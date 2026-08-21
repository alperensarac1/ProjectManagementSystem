namespace ProjectManagement.Application.DTOs.Mailbox;

/// <summary>
/// Mailbox mesajı gönderme işleminin Application
/// katmanındaki modelidir.
/// </summary>
public sealed class SendMailboxMessageDto
{
    /*
     * Mesajın gönderileceği kullanıcı kimlikleri.
     *
     * İstemciden alınan e-posta adresleri yerine kullanıcı ID'leri
     * kullanılır. Böylece kayıtlı olmayan bir adrese mesaj
     * gönderilmesi engellenir.
     */
    public IReadOnlyCollection<int> RecipientUserIds { get; init; } =
        Array.Empty<int>();

    public string Subject { get; init; } =
        string.Empty;

    public string Body { get; init; } =
        string.Empty;

    public IReadOnlyCollection<UploadedMailboxFileDto> Attachments
    { get; init; } =
        Array.Empty<UploadedMailboxFileDto>();
}