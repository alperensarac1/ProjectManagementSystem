namespace ProjectManagement.Application.Interfaces.Services;


public interface IMailboxAttachmentCleanupService
{
    Task<int> DeleteExpiredFilesAsync(
        CancellationToken cancellationToken = default);
}