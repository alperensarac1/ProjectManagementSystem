namespace ProjectManagement.Application.Mailbox;

/// <summary>
/// Mailbox dosya yükleme kurallarının merkezi olarak
/// tutulduğu sabit değerleri içerir.
/// </summary>
public static class MailboxFileConstants
{
    /*
     * Bir dosyanın maksimum boyutu:
     *
     * 200 MB = 200 × 1024 × 1024 byte
     */
    public const long MaximumFileSize =
        200L * 1024L * 1024L;

    /*
     * Tek mesajda bulunan bütün dosyaların toplam boyutu
     * en fazla 200 MB olabilir.
     */
    public const long MaximumTotalFileSize =
        200L * 1024L * 1024L;

    /*
     * Tek bir mesaja en fazla 10 dosya eklenebilir.
     */
    public const int MaximumAttachmentCount = 10;

    /*
     * Mesaj konusu için maksimum uzunluk.
     */
    public const int MaximumSubjectLength = 250;

    /*
     * Mesaj içeriği için maksimum uzunluk.
     */
    public const int MaximumBodyLength = 20_000;

    /*
     * Yüklenen dosyalar bir ay sonra silinecektir.
     */
    public const int AttachmentRetentionMonths = 1;

    /*
     * İzin verilen dosya uzantıları.
     */
    public static readonly IReadOnlySet<string>
        AllowedExtensions =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ".pdf",
                ".doc",
                ".docx",
                ".zip",
                ".png",
                ".jpg",
                ".jpeg"
            };

    /*
     * İzin verilen MIME türleri.
     *
     * Tarayıcılar ve işletim sistemleri ZIP/JPG gibi dosyalar için
     * farklı MIME türleri gönderebilir.
     */
    public static readonly IReadOnlySet<string>
        AllowedContentTypes =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                "application/pdf",

                "application/msword",

                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

                "application/zip",
                "application/x-zip-compressed",
                "multipart/x-zip",

                "image/png",

                "image/jpeg",
                "image/jpg"
            };
}