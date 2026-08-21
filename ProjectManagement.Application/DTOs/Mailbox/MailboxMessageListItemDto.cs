namespace ProjectManagement.Application.DTOs.Mailbox;

/// <summary>
/// Gelen ve gönderilen kutularındaki mesaj listesinin
/// tek bir satırını temsil eder.
/// </summary>
public sealed class MailboxMessageListItemDto
{
    public int Id { get; init; }

    public string Subject { get; init; } =
        string.Empty;

    /*
     * Mesaj içeriğinin liste ekranında gösterilecek
     * kısa ön izlemesidir.
     */
    public string BodyPreview { get; init; } =
        string.Empty;

    public MailboxUserDto Sender { get; init; } =
        new();

    public IReadOnlyCollection<MailboxUserDto> Recipients
    { get; init; } =
        Array.Empty<MailboxUserDto>();

    public DateTime SentAtUtc { get; init; }

    /*
     * Gönderilen kutusunda bu değer null olabilir.
     */
    public bool? IsRead { get; init; }

    public DateTime? ReadAtUtc { get; init; }

    public bool HasAttachment { get; init; }

    public int AttachmentCount { get; init; }
}