using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Mailbox;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Interfaces.Storage;
using ProjectManagement.Application.Mappings;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Services;

public sealed class MailboxService : IMailboxService
{
    private readonly IMailboxRepository _mailboxRepository;
    private readonly IMailboxFileStorage _fileStorage;

    public MailboxService(
        IMailboxRepository mailboxRepository,
        IMailboxFileStorage fileStorage)
    {
        _mailboxRepository = mailboxRepository;
        _fileStorage = fileStorage;
    }

    public async Task<MailboxMessageDetailDto> SendAsync(
        int senderUserId,
        SendMailboxMessageDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (senderUserId <= 0)
        {
            throw new BusinessRuleException(
                "Mesajı gönderen kullanıcı bilgisi geçersizdir.");
        }
        
        var recipientUserIds =
            request.RecipientUserIds
                .Distinct()
                .ToArray();

        if (recipientUserIds.Length == 0)
        {
            throw new BusinessRuleException(
                "En az bir alıcı seçilmelidir.");
        }
        
        var activeRecipients =
            await _mailboxRepository.GetActiveUsersByIdsAsync(
                recipientUserIds,
                cancellationToken);

        var activeRecipientIds =
            activeRecipients
                .Select(user => user.Id)
                .ToHashSet();

        var unavailableRecipientIds =
            recipientUserIds
                .Where(userId =>
                    !activeRecipientIds.Contains(userId))
                .ToArray();

        if (unavailableRecipientIds.Length > 0)
        {
            throw new BusinessRuleException(
                "Alıcılardan biri veya birkaçı bulunamadı ya da aktif değildir.");
        }
        
        var storedFiles =
            new List<
                ProjectManagement.Application.Interfaces.Storage
                    .StoredMailboxFile>();

        try
        {
            foreach (var attachment in request.Attachments)
            {
                var storedFile =
                    await _fileStorage.SaveAsync(
                        attachment,
                        cancellationToken);

                storedFiles.Add(storedFile);
            }

            var sentAtUtc =
                DateTime.UtcNow;

            var message = new MailboxMessage
            {
                SenderUserId = senderUserId,
                Subject = request.Subject.Trim(),
                Body = request.Body.Trim(),
                SentAtUtc = sentAtUtc,
                IsDeletedBySender = false
            };

            foreach (var recipientUserId in recipientUserIds)
            {
                message.Recipients.Add(
                    new MailboxRecipient
                    {
                        RecipientUserId =
                            recipientUserId,

                        IsRead =
                            false,

                        ReadAtUtc =
                            null,

                        IsDeletedByRecipient =
                            false
                    });
            }

            foreach (var storedFile in storedFiles)
            {
                message.Attachments.Add(
                    new MailboxAttachment
                    {
                        OriginalFileName =
                            storedFile.OriginalFileName,

                        StoredFileName =
                            storedFile.StoredFileName,

                        RelativePath =
                            storedFile.RelativePath,

                        ContentType =
                            storedFile.ContentType,

                        Extension =
                            storedFile.Extension,

                        FileSize =
                            storedFile.FileSize,

                        UploadedAtUtc =
                            storedFile.UploadedAtUtc,

                        ExpiresAtUtc =
                            storedFile.ExpiresAtUtc,

                        FileDeletedAtUtc =
                            null,

                        IsFileDeleted =
                            false
                    });
            }

            await _mailboxRepository.AddMessageAsync(
                message,
                cancellationToken);

            await _mailboxRepository.SaveChangesAsync(
                cancellationToken);
            
            var createdMessage =
                await _mailboxRepository.GetMessageDetailAsync(
                    message.Id,
                    cancellationToken);

            if (createdMessage is null)
            {
                throw new NotFoundException(
                    "Oluşturulan mesaj tekrar getirilemedi.");
            }

            return createdMessage.ToDetailDto(
                senderUserId);
        }
        catch
        {
            foreach (var storedFile in storedFiles)
            {
                try
                {
                    await _fileStorage.DeleteAsync(
                        storedFile.RelativePath,
                        CancellationToken.None);
                }
                catch
                {
                }
            }

            throw;
        }
    }

    public async Task<PagedResult<MailboxMessageListItemDto>>
        GetInboxAsync(
            int currentUserId,
            MailboxListQueryDto query,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var result =
            await _mailboxRepository.GetInboxAsync(
                currentUserId,
                query.Page,
                query.PageSize,
                NormalizeOptionalSearch(query.Search),
                query.IsRead,
                query.HasAttachment,
                cancellationToken);

        var items =
            result.Items
                .Select(recipient =>
                    recipient.ToInboxListItemDto())
                .ToArray();

        return PagedResult<MailboxMessageListItemDto>.Create(
            items,
            query.Page,
            query.PageSize,
            result.TotalCount);
    }

    public async Task<PagedResult<MailboxMessageListItemDto>>
        GetSentAsync(
            int currentUserId,
            MailboxListQueryDto query,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var result =
            await _mailboxRepository.GetSentAsync(
                currentUserId,
                query.Page,
                query.PageSize,
                NormalizeOptionalSearch(query.Search),
                query.HasAttachment,
                cancellationToken);

        var items =
            result.Items
                .Select(message =>
                    message.ToSentListItemDto())
                .ToArray();

        return PagedResult<MailboxMessageListItemDto>.Create(
            items,
            query.Page,
            query.PageSize,
            result.TotalCount);
    }

    public async Task<MailboxMessageDetailDto> GetByIdAsync(
        int messageId,
        int currentUserId,
        bool markAsRead,
        CancellationToken cancellationToken = default)
    {
        var message =
            await GetAccessibleMessageAsync(
                messageId,
                currentUserId,
                cancellationToken);

        var currentRecipient =
            message.Recipients.FirstOrDefault(
                recipient =>
                    recipient.RecipientUserId ==
                    currentUserId);

        if (markAsRead &&
            currentRecipient is not null &&
            !currentRecipient.IsRead)
        {
            await MarkAsReadAsync(
                messageId,
                currentUserId,
                cancellationToken);

            /*
             * message AsNoTracking ile yüklendiği için response modelinde
             * güncel değerin görünmesi amacıyla yerel nesneyi de güncelliyoruz.
             */
            currentRecipient.IsRead = true;
            currentRecipient.ReadAtUtc =
                DateTime.UtcNow;
        }

        return message.ToDetailDto(
            currentUserId);
    }

    public async Task MarkAsReadAsync(
        int messageId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var recipient =
            await _mailboxRepository.GetRecipientForUpdateAsync(
                messageId,
                currentUserId,
                cancellationToken);

        if (recipient is null ||
            recipient.IsDeletedByRecipient)
        {
            throw new NotFoundException(
                "Gelen kutusunda işaretlenecek mesaj bulunamadı.");
        }

        if (recipient.IsRead)
        {
            return;
        }

        recipient.IsRead = true;
        recipient.ReadAtUtc = DateTime.UtcNow;

        _mailboxRepository.UpdateRecipient(
            recipient);

        await _mailboxRepository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task MarkAsUnreadAsync(
        int messageId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var recipient =
            await _mailboxRepository.GetRecipientForUpdateAsync(
                messageId,
                currentUserId,
                cancellationToken);

        if (recipient is null ||
            recipient.IsDeletedByRecipient)
        {
            throw new NotFoundException(
                "Gelen kutusunda işaretlenecek mesaj bulunamadı.");
        }

        if (!recipient.IsRead)
        {
            return;
        }

        recipient.IsRead = false;
        recipient.ReadAtUtc = null;

        _mailboxRepository.UpdateRecipient(
            recipient);

        await _mailboxRepository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task DeleteAsync(
        int messageId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var message =
            await _mailboxRepository.GetMessageForUpdateAsync(
                messageId,
                cancellationToken);

        if (message is null)
        {
            throw new NotFoundException(
                "Silinecek mesaj bulunamadı.");
        }
        
        if (message.SenderUserId == currentUserId)
        {
            if (!message.IsDeletedBySender)
            {
                message.IsDeletedBySender = true;

                _mailboxRepository.UpdateMessage(
                    message);

                await _mailboxRepository.SaveChangesAsync(
                    cancellationToken);
            }

            return;
        }

        var recipient =
            message.Recipients.FirstOrDefault(
                item =>
                    item.RecipientUserId ==
                    currentUserId);
        
        if (recipient is null)
        {
            throw new BusinessRuleException(
                "Bu mesajı silme yetkiniz bulunmamaktadır.");
        }

        if (!recipient.IsDeletedByRecipient)
        {
            recipient.IsDeletedByRecipient = true;

            _mailboxRepository.UpdateRecipient(
                recipient);

            await _mailboxRepository.SaveChangesAsync(
                cancellationToken);
        }
    }

    public async Task<MailboxFileDownloadDto>
        DownloadAttachmentAsync(
            int messageId,
            int attachmentId,
            int currentUserId,
            CancellationToken cancellationToken = default)
    {
        await GetAccessibleMessageAsync(
            messageId,
            currentUserId,
            cancellationToken);

        var attachment =
            await _mailboxRepository.GetAttachmentAsync(
                attachmentId,
                messageId,
                cancellationToken);

        if (attachment is null)
        {
            throw new NotFoundException(
                "İndirilecek dosya eki bulunamadı.");
        }

        if (attachment.IsFileDeleted ||
            attachment.FileDeletedAtUtc.HasValue)
        {
            throw new NotFoundException(
                "Bu dosya fiziksel depolamadan silinmiştir.");
        }

        if (attachment.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "Bu dosyanın saklama süresi dolmuştur.");
        }

        var exists =
            await _fileStorage.ExistsAsync(
                attachment.RelativePath,
                cancellationToken);

        if (!exists)
        {
            throw new NotFoundException(
                "Dosya fiziksel depolamada bulunamadı.");
        }

        var storedFile =
            await _fileStorage.OpenReadAsync(
                attachment.RelativePath,
                cancellationToken);

        return new MailboxFileDownloadDto
        {
            Content = storedFile.Content,
            FileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            FileSize = storedFile.Length
        };
    }

    private async Task<MailboxMessage>
        GetAccessibleMessageAsync(
            int messageId,
            int currentUserId,
            CancellationToken cancellationToken)
    {
        var message =
            await _mailboxRepository.GetMessageDetailAsync(
                messageId,
                cancellationToken);

        if (message is null)
        {
            throw new NotFoundException(
                "Mailbox mesajı bulunamadı.");
        }

        var isSender =
            message.SenderUserId ==
            currentUserId;

        var recipient =
            message.Recipients.FirstOrDefault(
                item =>
                    item.RecipientUserId ==
                    currentUserId);

        var isRecipient =
            recipient is not null;

        if (!isSender &&
            !isRecipient)
        {
            throw new BusinessRuleException(
                "Bu mesajı görüntüleme yetkiniz bulunmamaktadır.");
        }
        
        if (isSender &&
            message.IsDeletedBySender)
        {
            throw new NotFoundException(
                "Mailbox mesajı bulunamadı.");
        }

        if (recipient is not null &&
            recipient.IsDeletedByRecipient)
        {
            throw new NotFoundException(
                "Mailbox mesajı bulunamadı.");
        }

        return message;
    }

    private static string? NormalizeOptionalSearch(
        string? search)
    {
        return string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();
    }
}