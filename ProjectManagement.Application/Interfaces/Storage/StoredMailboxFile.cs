namespace ProjectManagement.Application.Interfaces.Storage;

/// <summary>
/// Yerel depolama servisi tarafından başarıyla kaydedilen
/// bir dosyanın bilgilerini temsil eder.
/// </summary>
public sealed class StoredMailboxFile
{
    public string OriginalFileName { get; init; } =
        string.Empty;

    public string StoredFileName { get; init; } =
        string.Empty;

    public string RelativePath { get; init; } =
        string.Empty;

    public string ContentType { get; init; } =
        string.Empty;

    public string Extension { get; init; } =
        string.Empty;

    public long FileSize { get; init; }

    public DateTime UploadedAtUtc { get; init; }

    public DateTime ExpiresAtUtc { get; init; }
}