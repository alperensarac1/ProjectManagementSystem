namespace ProjectManagement.Application.DTOs.Mailbox;

/// <summary>
/// Controller tarafından FileStreamResult oluşturulurken
/// kullanılacak dosya indirme modelidir.
/// </summary>
public sealed class MailboxFileDownloadDto
{
    public Stream Content { get; init; } =
        Stream.Null;

    public string FileName { get; init; } =
        string.Empty;

    public string ContentType { get; init; } =
        "application/octet-stream";

    public long FileSize { get; init; }
}