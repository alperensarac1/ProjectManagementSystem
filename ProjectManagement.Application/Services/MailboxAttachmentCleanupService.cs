using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Interfaces.Storage;
using ProjectManagement.Application.Mailbox;

namespace ProjectManagement.Application.Services;

public sealed class MailboxAttachmentCleanupService
    : IMailboxAttachmentCleanupService
{
    private readonly IMailboxRepository _mailboxRepository;
    private readonly IMailboxFileStorage _fileStorage;

    public MailboxAttachmentCleanupService(
        IMailboxRepository mailboxRepository,
        IMailboxFileStorage fileStorage)
    {
        _mailboxRepository = mailboxRepository;
        _fileStorage = fileStorage;
    }

    public async Task<int> DeleteExpiredFilesAsync(
        CancellationToken cancellationToken = default)
    {
        var totalProcessedCount = 0;

      
        for (var batchNumber = 1;
             batchNumber <=
             MailboxCleanupConstants.MaximumBatchCountPerRun;
             batchNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var utcNow =
                DateTime.UtcNow;

            var expiredAttachments =
                await _mailboxRepository
                    .GetExpiredAttachmentsForUpdateAsync(
                        utcNow,
                        MailboxCleanupConstants.BatchSize,
                        cancellationToken);

            if (expiredAttachments.Count == 0)
            {
                break;
            }

            foreach (var attachment in expiredAttachments)
            {
                cancellationToken.ThrowIfCancellationRequested();

              
                var fileExists =
                    await _fileStorage.ExistsAsync(
                        attachment.RelativePath,
                        cancellationToken);

                if (fileExists)
                {
                   
                    await _fileStorage.DeleteAsync(
                        attachment.RelativePath,
                        cancellationToken);

                    
                    var stillExists =
                        await _fileStorage.ExistsAsync(
                            attachment.RelativePath,
                            cancellationToken);

                    if (stillExists)
                    {
                        throw new IOException(
                            $"Mailbox dosyası fiziksel depolamadan silinemedi. " +
                            $"AttachmentId: {attachment.Id}");
                    }
                }
                
                attachment.IsFileDeleted =
                    true;

                attachment.FileDeletedAtUtc =
                    DateTime.UtcNow;

                _mailboxRepository.UpdateAttachment(
                    attachment);

                totalProcessedCount++;
            }
            
            await _mailboxRepository.SaveChangesAsync(
                cancellationToken);
            
            if (expiredAttachments.Count <
                MailboxCleanupConstants.BatchSize)
            {
                break;
            }
        }

        return totalProcessedCount;
    }
}