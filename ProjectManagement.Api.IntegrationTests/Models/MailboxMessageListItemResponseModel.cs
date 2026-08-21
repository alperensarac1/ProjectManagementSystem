namespace ProjectManagement.Api.IntegrationTests.Models;

public sealed class MailboxMessageListItemResponseModel
{
    public int Id { get; init; }

    public string Subject { get; init; } =
        string.Empty;

    public string BodyPreview { get; init; } =
        string.Empty;

    public MailboxUserResponseModel Sender { get; init; } =
        new();

    public IReadOnlyCollection<MailboxUserResponseModel>
        Recipients { get; init; } =
        Array.Empty<MailboxUserResponseModel>();

    public DateTime SentAtUtc { get; init; }

    public bool? IsRead { get; init; }

    public DateTime? ReadAtUtc { get; init; }

    public bool HasAttachment { get; init; }

    public int AttachmentCount { get; init; }
}