using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Interfaces.Repositories;

/// <summary>
/// Mailbox veritabanı işlemlerini tanımlar.
/// </summary>
public interface IMailboxRepository
{
    Task AddMessageAsync(
        MailboxMessage message,
        CancellationToken cancellationToken = default);

    Task<MailboxMessage?> GetMessageDetailAsync(
        int messageId,
        CancellationToken cancellationToken = default);

    Task<MailboxMessage?> GetMessageForUpdateAsync(
        int messageId,
        CancellationToken cancellationToken = default);

    Task<MailboxRecipient?> GetRecipientForUpdateAsync(
        int messageId,
        int recipientUserId,
        CancellationToken cancellationToken = default);

    Task<MailboxAttachment?> GetAttachmentAsync(
        int attachmentId,
        int messageId,
        CancellationToken cancellationToken = default);

    Task<MailboxAttachment?> GetAttachmentForUpdateAsync(
        int attachmentId,
        CancellationToken cancellationToken = default);

    Task<(
        IReadOnlyCollection<MailboxRecipient> Items,
        int TotalCount)> GetInboxAsync(
            int userId,
            int page,
            int pageSize,
            string? search,
            bool? isRead,
            bool? hasAttachment,
            CancellationToken cancellationToken = default);

    Task<(
        IReadOnlyCollection<MailboxMessage> Items,
        int TotalCount)> GetSentAsync(
            int userId,
            int page,
            int pageSize,
            string? search,
            bool? hasAttachment,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MailboxAttachment>>
        GetExpiredAttachmentsForUpdateAsync(
            DateTime utcNow,
            int take,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<User>>
        GetActiveUsersByIdsAsync(
            IReadOnlyCollection<int> userIds,
            CancellationToken cancellationToken = default);

    void UpdateMessage(
        MailboxMessage message);

    void UpdateRecipient(
        MailboxRecipient recipient);

    void UpdateAttachment(
        MailboxAttachment attachment);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}