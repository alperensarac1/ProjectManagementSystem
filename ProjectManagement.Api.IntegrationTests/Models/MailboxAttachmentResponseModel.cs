namespace ProjectManagement.Api.IntegrationTests.Models;

public sealed class MailboxAttachmentResponseModel
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

    public bool IsAvailable { get; init; }
}