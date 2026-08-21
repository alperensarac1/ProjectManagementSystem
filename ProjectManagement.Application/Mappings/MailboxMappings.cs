using ProjectManagement.Application.DTOs.Mailbox;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Mappings;


public static class MailboxMappings
{
    public static MailboxUserDto ToMailboxUserDto(
        this User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new MailboxUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName =
                $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email
        };
    }

    public static MailboxAttachmentResponseDto
        ToMailboxAttachmentResponseDto(
            this MailboxAttachment attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        var isAvailable =
            !attachment.IsFileDeleted &&
            attachment.FileDeletedAtUtc is null &&
            attachment.ExpiresAtUtc > DateTime.UtcNow;

        return new MailboxAttachmentResponseDto
        {
            Id = attachment.Id,
            MessageId = attachment.MessageId,
            OriginalFileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            Extension = attachment.Extension,
            FileSize = attachment.FileSize,
            UploadedAtUtc = attachment.UploadedAtUtc,
            ExpiresAtUtc = attachment.ExpiresAtUtc,
            FileDeletedAtUtc = attachment.FileDeletedAtUtc,
            IsFileDeleted = attachment.IsFileDeleted,
            IsAvailable = isAvailable
        };
    }
    
    public static MailboxMessageListItemDto ToInboxListItemDto(
        this MailboxRecipient recipient)
    {
        ArgumentNullException.ThrowIfNull(recipient);

        var message =
            recipient.Message
            ?? throw new InvalidOperationException(
                "Gelen kutusu mesaj bilgisi yüklenemedi.");

        var sender =
            message.SenderUser
            ?? throw new InvalidOperationException(
                "Mesajı gönderen kullanıcı bilgisi yüklenemedi.");

        var recipients =
            message.Recipients
            ?? new List<MailboxRecipient>();

        var attachments =
            message.Attachments
            ?? new List<MailboxAttachment>();

        return new MailboxMessageListItemDto
        {
            Id = message.Id,
            Subject = message.Subject,
            BodyPreview = CreateBodyPreview(
                message.Body),

            Sender =
                sender.ToMailboxUserDto(),

            Recipients =
                recipients
                    .Where(item =>
                        item.RecipientUser is not null)
                    .OrderBy(item =>
                        item.RecipientUser.FirstName)
                    .ThenBy(item =>
                        item.RecipientUser.LastName)
                    .Select(item =>
                        item.RecipientUser.ToMailboxUserDto())
                    .ToArray(),

            SentAtUtc =
                message.SentAtUtc,

            IsRead =
                recipient.IsRead,

            ReadAtUtc =
                recipient.ReadAtUtc,

            HasAttachment =
                attachments.Count > 0,

            AttachmentCount =
                attachments.Count
        };
    }
    
    public static MailboxMessageListItemDto ToSentListItemDto(
        this MailboxMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new MailboxMessageListItemDto
        {
            Id = message.Id,
            Subject = message.Subject,
            BodyPreview = CreateBodyPreview(message.Body),
            Sender = message.SenderUser.ToMailboxUserDto(),

            Recipients = message.Recipients
                .OrderBy(item =>
                    item.RecipientUser.FirstName)
                .ThenBy(item =>
                    item.RecipientUser.LastName)
                .Select(item =>
                    item.RecipientUser.ToMailboxUserDto())
                .ToArray(),

            SentAtUtc = message.SentAtUtc,
            
            IsRead = null,
            ReadAtUtc = null,

            HasAttachment = message.Attachments.Count > 0,
            AttachmentCount = message.Attachments.Count
        };
    }

    public static MailboxMessageDetailDto ToDetailDto(
        this MailboxMessage message,
        int currentUserId)
    {
        ArgumentNullException.ThrowIfNull(message);

        var currentRecipient =
            message.Recipients.FirstOrDefault(
                recipient =>
                    recipient.RecipientUserId ==
                    currentUserId);

        return new MailboxMessageDetailDto
        {
            Id = message.Id,
            Subject = message.Subject,
            Body = message.Body,
            Sender = message.SenderUser.ToMailboxUserDto(),

            Recipients = message.Recipients
                .OrderBy(recipient =>
                    recipient.RecipientUser.FirstName)
                .ThenBy(recipient =>
                    recipient.RecipientUser.LastName)
                .Select(recipient =>
                    recipient.RecipientUser
                        .ToMailboxUserDto())
                .ToArray(),

            SentAtUtc = message.SentAtUtc,
            
            IsRead = currentRecipient?.IsRead,
            ReadAtUtc = currentRecipient?.ReadAtUtc,

            Attachments = message.Attachments
                .OrderBy(attachment =>
                    attachment.OriginalFileName)
                .Select(attachment =>
                    attachment
                        .ToMailboxAttachmentResponseDto())
                .ToArray()
        };
    }

    private static string CreateBodyPreview(
        string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }
        
        var normalizedBody =
            body
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();

        const int maximumLength = 150;

        if (normalizedBody.Length <= maximumLength)
        {
            return normalizedBody;
        }

        return
            normalizedBody[..maximumLength]
            + "...";
    }
}