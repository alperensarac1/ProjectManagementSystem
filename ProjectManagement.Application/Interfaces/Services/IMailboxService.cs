using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Mailbox;

namespace ProjectManagement.Application.Interfaces.Services;


public interface IMailboxService
{
  
    Task<MailboxMessageDetailDto> SendAsync(
        int senderUserId,
        SendMailboxMessageDto request,
        CancellationToken cancellationToken = default);
    
    Task<PagedResult<MailboxMessageListItemDto>> GetInboxAsync(
        int currentUserId,
        MailboxListQueryDto query,
        CancellationToken cancellationToken = default);

    Task<PagedResult<MailboxMessageListItemDto>> GetSentAsync(
        int currentUserId,
        MailboxListQueryDto query,
        CancellationToken cancellationToken = default);
    
    Task<MailboxMessageDetailDto> GetByIdAsync(
        int messageId,
        int currentUserId,
        bool markAsRead,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(
        int messageId,
        int currentUserId,
        CancellationToken cancellationToken = default);


    Task MarkAsUnreadAsync(
        int messageId,
        int currentUserId,
        CancellationToken cancellationToken = default);
    
    Task DeleteAsync(
        int messageId,
        int currentUserId,
        CancellationToken cancellationToken = default);
    
    Task<MailboxFileDownloadDto> DownloadAttachmentAsync(
        int messageId,
        int attachmentId,
        int currentUserId,
        CancellationToken cancellationToken = default);
}