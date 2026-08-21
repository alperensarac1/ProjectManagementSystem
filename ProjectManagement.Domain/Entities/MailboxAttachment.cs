using ProjectManagement.Domain.Common;

namespace ProjectManagement.Domain.Entities;

/// <summary>
/// Mailbox mesajına eklenmiş fiziksel bir dosyanın
/// veritabanındaki metadata kaydını temsil eder.
///
/// Dosyanın byte içeriği SQLite içine kaydedilmez.
/// Gerçek dosya yerel depolama klasöründe tutulur.
/// </summary>
public sealed class MailboxAttachment : BaseEntity
{
    public int MessageId { get; set; }

    /*
     * Kullanıcının yüklediği orijinal dosya adı.
     *
     * Örnek:
     * aylik-rapor.pdf
     */
    public string OriginalFileName { get; set; } =
        string.Empty;

    /*
     * Sunucuda kullanılan güvenli ve benzersiz dosya adı.
     *
     * Örnek:
     * 3cb70ae0f8024b2db3c9bb332a4135c3.pdf
     */
    public string StoredFileName { get; set; } =
        string.Empty;

    /*
     * Ana yükleme klasörüne göre göreceli dosya yolu.
     *
     * Örnek:
     * 2026/08/3cb70ae0f8024b2db3c9bb332a4135c3.pdf
     *
     * Tam Windows veya Linux yolu veritabanında tutulmaz.
     * Böylece uygulama farklı ortamlara taşınabilir.
     */
    public string RelativePath { get; set; } =
        string.Empty;

    /*
     * Tarayıcı veya istemci tarafından bildirilen MIME türü.
     *
     * Bu alana tek başına güvenilmeyecek; dosya uzantısı ve
     * dosya imzası da doğrulanacak.
     */
    public string ContentType { get; set; } =
        string.Empty;

    /*
     * Nokta dâhil küçük harfli uzantı.
     *
     * Örnek:
     * .pdf
     * .docx
     * .png
     * .jpg
     */
    public string Extension { get; set; } =
        string.Empty;

    /*
     * Dosya boyutu byte olarak tutulur.
     */
    public long FileSize { get; set; }

    /*
     * Dosyanın fiziksel depolamaya yazıldığı UTC zamanı.
     */
    public DateTime UploadedAtUtc { get; set; }

    /*
     * Dosyanın fiziksel depolamadan silinebileceği UTC zamanı.
     *
     * Oluşturulurken UploadedAtUtc.AddMonths(1) atanacaktır.
     */
    public DateTime ExpiresAtUtc { get; set; }

    /*
     * Fiziksel dosyanın temizlendiği UTC zamanı.
     */
    public DateTime? FileDeletedAtUtc { get; set; }

    /*
     * Fiziksel dosya bir aylık süre sonunda silindi mi?
     *
     * Metadata kaydı geçmiş bilgisi olarak veritabanında kalır.
     */
    public bool IsFileDeleted { get; set; }

    public MailboxMessage Message { get; set; } =
        null!;
}