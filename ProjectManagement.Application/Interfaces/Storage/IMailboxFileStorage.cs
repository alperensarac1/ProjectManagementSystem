using ProjectManagement.Application.DTOs.Mailbox;

namespace ProjectManagement.Application.Interfaces.Storage;

/// <summary>
/// Mailbox dosyalarının fiziksel depolama işlemlerini
/// soyutlar.
///
/// Application katmanı dosyanın Windows, Linux, Docker volume
/// veya başka bir depolamada bulunduğunu bilmez.
/// </summary>
public interface IMailboxFileStorage
{
    Task<StoredMailboxFile> SaveAsync(
        UploadedMailboxFileDto file,
        CancellationToken cancellationToken = default);

    Task<StoredMailboxFileStream> OpenReadAsync(
        string relativePath,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string relativePath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken = default);
}