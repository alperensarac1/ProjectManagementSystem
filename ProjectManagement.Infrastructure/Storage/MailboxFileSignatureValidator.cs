using ProjectManagement.Application.Common.Exceptions;

namespace ProjectManagement.Infrastructure.Storage;

/// <summary>
/// Yüklenen dosyaların dosya imzasını, yani magic byte
/// değerlerini doğrular.
/// </summary>
internal static class MailboxFileSignatureValidator
{
    public static async Task ValidateAsync(
        Stream stream,
        string extension,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            throw new BusinessRuleException(
                "Yüklenen dosya okunamıyor.");
        }

        /*
         * En uzun kontrol için ilk 12 byte yeterlidir.
         */
        var header = new byte[12];

        var originalPosition =
            stream.CanSeek
                ? stream.Position
                : 0;

        var bytesRead =
            await stream.ReadAsync(
                header.AsMemory(0, header.Length),
                cancellationToken);

        if (stream.CanSeek)
        {
            stream.Position =
                originalPosition;
        }

        if (bytesRead < 2)
        {
            throw new BusinessRuleException(
                "Dosya içeriği geçersiz veya eksiktir.");
        }

        var normalizedExtension =
            extension.ToLowerInvariant();

        var isValid =
            normalizedExtension switch
            {
                ".pdf" =>
                    StartsWith(
                        header,
                        bytesRead,
                        [0x25, 0x50, 0x44, 0x46]),

                ".png" =>
                    StartsWith(
                        header,
                        bytesRead,
                        [
                            0x89,
                            0x50,
                            0x4E,
                            0x47,
                            0x0D,
                            0x0A,
                            0x1A,
                            0x0A
                        ]),

                ".jpg" or ".jpeg" =>
                    StartsWith(
                        header,
                        bytesRead,
                        [0xFF, 0xD8, 0xFF]),

                /*
                 * DOC eski OLE Compound File formatını kullanır.
                 */
                ".doc" =>
                    StartsWith(
                        header,
                        bytesRead,
                        [
                            0xD0,
                            0xCF,
                            0x11,
                            0xE0,
                            0xA1,
                            0xB1,
                            0x1A,
                            0xE1
                        ]),

                /*
                 * DOCX ve ZIP dosyaları ZIP tabanlıdır.
                 *
                 * PK 03 04: normal ZIP
                 * PK 05 06: boş ZIP
                 * PK 07 08: split ZIP
                 */
                ".docx" or ".zip" =>
                    IsZipSignature(
                        header,
                        bytesRead),

                _ => false
            };

        if (!isValid)
        {
            throw new BusinessRuleException(
                "Dosyanın içeriği ile uzantısı uyuşmuyor.");
        }
    }

    private static bool IsZipSignature(
        byte[] header,
        int bytesRead)
    {
        return
            StartsWith(
                header,
                bytesRead,
                [0x50, 0x4B, 0x03, 0x04]) ||
            StartsWith(
                header,
                bytesRead,
                [0x50, 0x4B, 0x05, 0x06]) ||
            StartsWith(
                header,
                bytesRead,
                [0x50, 0x4B, 0x07, 0x08]);
    }

    private static bool StartsWith(
        byte[] source,
        int sourceLength,
        byte[] signature)
    {
        if (sourceLength < signature.Length)
        {
            return false;
        }

        for (var index = 0;
             index < signature.Length;
             index++)
        {
            if (source[index] != signature[index])
            {
                return false;
            }
        }

        return true;
    }
}