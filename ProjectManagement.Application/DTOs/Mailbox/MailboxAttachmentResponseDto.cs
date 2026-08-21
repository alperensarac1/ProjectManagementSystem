namespace ProjectManagement.Application.DTOs.Mailbox;

/// <summary>
/// Mailbox mesajındaki dosya ekinin istemciye döndürülen
/// bilgilerini temsil eder.
/// </summary>
public sealed class MailboxAttachmentResponseDto
{
    public int Id { get; init; }

    public int MessageId { get; init; }

    public string OriginalFileName { get; init; } =
        string.Empty;

    public string ContentType { get; init; } =
        string.Empty;

    public string Extension { get; init; } =
        string.Empty;

    public long FileSize { get; init; }

    public DateTime UploadedAtUtc { get; init; }

    public DateTime ExpiresAtUtc { get; init; }

    public DateTime? FileDeletedAtUtc { get; init; }

    public bool IsFileDeleted { get; init; }

    /*
     * Dosya fiziksel olarak hâlâ mevcutsa ve süresi dolmamışsa
     * true olacaktır.
     */
    public bool IsAvailable { get; init; }
}