namespace ProjectManagement.Application.DTOs.Mailbox;

/// <summary>
/// Bir mailbox mesajının ayrıntılı görüntüsünü temsil eder.
/// </summary>
public sealed class MailboxMessageDetailDto
{
    public int Id { get; init; }

    public string Subject { get; init; } =
        string.Empty;

    public string Body { get; init; } =
        string.Empty;

    public MailboxUserDto Sender { get; init; } =
        new();

    public IReadOnlyCollection<MailboxUserDto> Recipients
    { get; init; } =
        Array.Empty<MailboxUserDto>();

    public DateTime SentAtUtc { get; init; }

    public bool? IsRead { get; init; }

    public DateTime? ReadAtUtc { get; init; }

    public IReadOnlyCollection<MailboxAttachmentResponseDto>
        Attachments { get; init; } =
        Array.Empty<MailboxAttachmentResponseDto>();
}