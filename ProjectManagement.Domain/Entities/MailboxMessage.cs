using ProjectManagement.Domain.Common;

namespace ProjectManagement.Domain.Entities;


/// <summary>
/// Uygulama içinde kullanıcılar arasında gönderilen
/// bir mailbox mesajını temsil eder.
/// </summary>
public sealed class MailboxMessage : BaseEntity
{
    /*
     * Mesajı gönderen kullanıcının kimliği.
     */
    public int SenderUserId { get; set; }

    /*
     * Mesaj başlığı.
     */
    public string Subject { get; set; } =
        string.Empty;

    /*
     * Mesaj içeriği.
     *
     * İlk sürümde düz metin olarak saklayacağız.
     * HTML içerik kabul etmeyerek XSS riskini azaltıyoruz.
     */
    public string Body { get; set; } =
        string.Empty;

    /*
     * Mesajın kullanıcı tarafından gönderildiği UTC zamanı.
     */
    public DateTime SentAtUtc { get; set; }

    /*
     * Gönderen mesajı kendi gönderilen kutusundan kaldırdı mı?
     *
     * Bu alan mesajı alıcıların gelen kutusundan kaldırmaz.
     */
    public bool IsDeletedBySender { get; set; }

    public User SenderUser { get; set; } =
        null!;

    /*
     * Mesajın alıcıları.
     */
    public ICollection<MailboxRecipient> Recipients { get; set; } =
        new List<MailboxRecipient>();

    /*
     * Mesaja eklenen dosyalar.
     */
    public ICollection<MailboxAttachment> Attachments { get; set; } =
        new List<MailboxAttachment>();
}