namespace ProjectManagement.Infrastructure.Storage;

/// <summary>
/// Mailbox dosyalarının fiziksel olarak saklanacağı
/// yerel depolama ayarlarını temsil eder.
/// </summary>
public sealed class MailboxStorageSettings
{
    public const string SectionName =
        "MailboxStorage";

    /*
     * Container içinde örnek:
     *
     * /app/uploads/mailbox
     *
     * Docker kullanılmadan çalıştırıldığında örnek:
     *
     * uploads/mailbox
     */
    public string RootDirectory { get; set; } =
        "uploads/mailbox";

    /*
     * Yüklenen dosyaların kaç ay sonra silineceği.
     */
    public int RetentionMonths { get; set; } = 1;

    /*
     * Temizlik servisinin kaç saatte bir çalışacağı.
     */
    public int CleanupIntervalHours { get; set; } = 6;

    /*
     * Süresi dolmuş dosyaların otomatik olarak
     * fiziksel depolamadan silinip silinmeyeceği.
     */
    public bool CleanupEnabled { get; set; } = true;
}