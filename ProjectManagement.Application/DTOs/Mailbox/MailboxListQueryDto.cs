namespace ProjectManagement.Application.DTOs.Mailbox;

/// <summary>
/// Gelen ve gönderilen mesaj kutularının sayfalı
/// sorgu parametrelerini temsil eder.
/// </summary>
public sealed class MailboxListQueryDto
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    /*
     * Gönderen adı, gönderen e-postası,
     * konu veya mesaj içeriğinde arama yapılabilir.
     */
    public string? Search { get; set; }

    /*
     * Gelen kutusunda yalnızca okundu veya okunmadı
     * mesajları filtrelemek için kullanılır.
     *
     * null olduğunda bütün mesajlar getirilir.
     */
    public bool? IsRead { get; set; }

    /*
     * Dosya eki bulunan mesajları filtrelemek için kullanılır.
     */
    public bool? HasAttachment { get; set; }
}