using ProjectManagement.Domain.Common;

namespace ProjectManagement.Domain.Entities;

/// <summary>
/// Bir mailbox mesajı ile mesajı alan kullanıcı
/// arasındaki ilişkiyi temsil eder.
/// </summary>
public sealed class MailboxRecipient : BaseEntity
{
    public int MessageId { get; set; }

    public int RecipientUserId { get; set; }

    /*
     * Mesaj alıcı tarafından okundu mu?
     */
    public bool IsRead { get; set; }

    /*
     * Mesajın ilk kez okunduğu UTC zamanı.
     */
    public DateTime? ReadAtUtc { get; set; }

    /*
     * Alıcı mesajı kendi gelen kutusundan kaldırdı mı?
     *
     * Bu alan mesajı gönderenin veya diğer alıcıların
     * kutusundan kaldırmaz.
     */
    public bool IsDeletedByRecipient { get; set; }

    public MailboxMessage Message { get; set; } =
        null!;

    public User RecipientUser { get; set; } =
        null!;
}