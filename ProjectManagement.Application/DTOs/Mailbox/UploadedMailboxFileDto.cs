namespace ProjectManagement.Application.DTOs.Mailbox;

/// <summary>
/// API katmanından Application katmanına aktarılan
/// dosya bilgisini temsil eder.
///
/// Bu sınıf IFormFile kullanmaz. Böylece Application katmanı
/// ASP.NET Core bağımlılığı taşımaz.
/// </summary>
public sealed class UploadedMailboxFileDto
{
    /*
     * Kullanıcının bilgisayarındaki orijinal dosya adı.
     */
    public string FileName { get; init; } =
        string.Empty;

    /*
     * İstemci tarafından bildirilen MIME türü.
     */
    public string ContentType { get; init; } =
        string.Empty;

    /*
     * Dosyanın byte cinsinden boyutu.
     */
    public long Length { get; init; }

    /*
     * Dosya içeriğinin okunacağı stream.
     *
     * MailboxService bu stream'i doğrudan yerel depolama
     * servisine aktaracaktır.
     */
    public Stream Content { get; init; } =
        Stream.Null;
}