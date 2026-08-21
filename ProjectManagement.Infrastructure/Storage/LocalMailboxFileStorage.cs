using Microsoft.Extensions.Options;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.DTOs.Mailbox;
using ProjectManagement.Application.Interfaces.Storage;
using ProjectManagement.Application.Mailbox;

namespace ProjectManagement.Infrastructure.Storage;

/// <summary>
/// Mailbox dosyalarını yerel dosya sistemine kaydeder.
///
/// Docker kullanıldığında RootDirectory bir volume veya
/// bind mount klasörüne yönlendirilmelidir.
/// </summary>
public sealed class LocalMailboxFileStorage
    : IMailboxFileStorage
{
    private readonly MailboxStorageSettings _settings;

    private readonly string _rootDirectory;

    public LocalMailboxFileStorage(
        IOptions<MailboxStorageSettings> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(
                _settings.RootDirectory))
        {
            throw new InvalidOperationException(
                "Mailbox dosya depolama klasörü tanımlanmamış.");
        }

        /*
         * Göreceli bir yol verilirse uygulamanın çalışma
         * klasörüne göre tam yola dönüştürülür.
         */
        _rootDirectory =
            Path.GetFullPath(
                _settings.RootDirectory);

        Directory.CreateDirectory(
            _rootDirectory);
    }

    public async Task<StoredMailboxFile> SaveAsync(
        UploadedMailboxFileDto file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        ValidateBasicFileProperties(
            file);

        /*
         * Path.GetFileName kullanarak istemcinin
         * "..\..\dosya" gibi path traversal içeren
         * dosya adı göndermesini engelliyoruz.
         */
        var safeOriginalFileName =
            Path.GetFileName(
                file.FileName.Trim());

        var extension =
            Path.GetExtension(
                    safeOriginalFileName)
                .ToLowerInvariant();

        await MailboxFileSignatureValidator.ValidateAsync(
            file.Content,
            extension,
            cancellationToken);

        var uploadedAtUtc =
            DateTime.UtcNow;

        var expiresAtUtc =
            uploadedAtUtc.AddMonths(
                GetRetentionMonths());

        /*
         * Dosyaları yıl/ay klasörlerine ayırıyoruz.
         *
         * Örnek:
         * 2026/08
         */
        var relativeDirectory =
            Path.Combine(
                uploadedAtUtc.Year.ToString("0000"),
                uploadedAtUtc.Month.ToString("00"));

        var physicalDirectory =
            GetSafePhysicalPath(
                relativeDirectory);

        Directory.CreateDirectory(
            physicalDirectory);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var relativePath =
            Path.Combine(
                relativeDirectory,
                storedFileName);

        var physicalPath =
            GetSafePhysicalPath(
                relativePath);

        try
        {
            /*
             * FileMode.CreateNew aynı dosya zaten varsa üzerine
             * yazılmasını engeller.
             */
            await using var outputStream =
                new FileStream(
                    physicalPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81_920,
                    options:
                        FileOptions.Asynchronous |
                        FileOptions.SequentialScan);

            if (file.Content.CanSeek)
            {
                file.Content.Position = 0;
            }

            /*
             * Dosya belleğe alınmadan doğrudan diske aktarılır.
             */
            await file.Content.CopyToAsync(
                outputStream,
                bufferSize: 81_920,
                cancellationToken);

            await outputStream.FlushAsync(
                cancellationToken);
        }
        catch
        {
            DeleteFileIfExists(
                physicalPath);

            throw;
        }

        var storedFileInfo =
            new FileInfo(
                physicalPath);

        storedFileInfo.Refresh();

        if (!storedFileInfo.Exists ||
            storedFileInfo.Length <= 0)
        {
            DeleteFileIfExists(
                physicalPath);

            throw new BusinessRuleException(
                "Dosya fiziksel depolamaya kaydedilemedi.");
        }

        /*
         * İstemcinin bildirdiği boyut ile gerçekten diske yazılan
         * boyutun aynı olduğundan emin oluyoruz.
         */
        if (storedFileInfo.Length != file.Length)
        {
            DeleteFileIfExists(
                physicalPath);

            throw new BusinessRuleException(
                "Dosyanın yüklenen boyutu ile kaydedilen boyutu uyuşmuyor.");
        }

        return new StoredMailboxFile
        {
            OriginalFileName =
                safeOriginalFileName,

            StoredFileName =
                storedFileName,

            /*
             * Veritabanında işletim sisteminden bağımsız olarak
             * "/" karakteriyle tutuyoruz.
             */
            RelativePath =
                NormalizeRelativePath(
                    relativePath),

            ContentType =
                NormalizeContentType(
                    file.ContentType),

            Extension =
                extension,

            FileSize =
                storedFileInfo.Length,

            UploadedAtUtc =
                uploadedAtUtc,

            ExpiresAtUtc =
                expiresAtUtc
        };
    }

    public Task<StoredMailboxFileStream> OpenReadAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var physicalPath =
            GetSafePhysicalPath(
                relativePath);

        if (!File.Exists(physicalPath))
        {
            throw new NotFoundException(
                "İndirilecek dosya fiziksel depolamada bulunamadı.");
        }

        var fileInfo =
            new FileInfo(
                physicalPath);

        var stream =
            new FileStream(
                physicalPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81_920,
                options:
                    FileOptions.Asynchronous |
                    FileOptions.SequentialScan);

        return Task.FromResult(
            new StoredMailboxFileStream
            {
                Content = stream,
                Length = fileInfo.Length
            });
    }

    public Task<bool> ExistsAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var physicalPath =
            GetSafePhysicalPath(
                relativePath);

        return Task.FromResult(
            File.Exists(physicalPath));
    }

    public Task DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var physicalPath =
            GetSafePhysicalPath(
                relativePath);

        DeleteFileIfExists(
            physicalPath);

        return Task.CompletedTask;
    }

    private static void ValidateBasicFileProperties(
        UploadedMailboxFileDto file)
    {
        if (string.IsNullOrWhiteSpace(
                file.FileName))
        {
            throw new BusinessRuleException(
                "Dosya adı boş olamaz.");
        }

        if (file.Length <= 0)
        {
            throw new BusinessRuleException(
                "Boş dosya yüklenemez.");
        }

        if (file.Length >
            MailboxFileConstants.MaximumFileSize)
        {
            throw new BusinessRuleException(
                "Bir dosyanın boyutu 200 MB'ı geçemez.");
        }

        var extension =
            Path.GetExtension(
                file.FileName);

        if (string.IsNullOrWhiteSpace(extension) ||
            !MailboxFileConstants
                .AllowedExtensions
                .Contains(extension))
        {
            throw new BusinessRuleException(
                "Bu dosya uzantısına izin verilmiyor.");
        }

        if (!MailboxFileConstants
                .AllowedContentTypes
                .Contains(file.ContentType))
        {
            throw new BusinessRuleException(
                "Bu dosya içerik türüne izin verilmiyor.");
        }

        if (file.Content is null ||
            !file.Content.CanRead)
        {
            throw new BusinessRuleException(
                "Dosya içeriği okunamıyor.");
        }
    }

    private int GetRetentionMonths()
    {
        return _settings.RetentionMonths > 0
            ? _settings.RetentionMonths
            : MailboxFileConstants.AttachmentRetentionMonths;
    }

    private string GetSafePhysicalPath(
        string relativePath)
    {
        if (string.IsNullOrWhiteSpace(
                relativePath))
        {
            throw new BusinessRuleException(
                "Dosya yolu geçersizdir.");
        }

        /*
         * Veritabanında "/" olarak saklanan yolu geçerli
         * işletim sistemi ayırıcısına dönüştürüyoruz.
         */
        var normalizedRelativePath =
            relativePath
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar)
                .Replace(
                    '\\',
                    Path.DirectorySeparatorChar);

        var physicalPath =
            Path.GetFullPath(
                Path.Combine(
                    _rootDirectory,
                    normalizedRelativePath));

        /*
         * Path traversal koruması.
         *
         * Oluşan tam yol mutlaka root klasörünün altında olmalıdır.
         */
        var normalizedRoot =
            _rootDirectory.EndsWith(
                Path.DirectorySeparatorChar)
                ? _rootDirectory
                : _rootDirectory +
                  Path.DirectorySeparatorChar;

        if (!physicalPath.StartsWith(
                normalizedRoot,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException(
                "Dosya yolu güvenli depolama klasörünün dışına çıkamaz.");
        }

        return physicalPath;
    }

    private static string NormalizeRelativePath(
        string relativePath)
    {
        return relativePath.Replace(
            Path.DirectorySeparatorChar,
            '/');
    }

    private static string NormalizeContentType(
        string contentType)
    {
        return contentType
            .Trim()
            .ToLowerInvariant();
    }

    private static void DeleteFileIfExists(
        string physicalPath)
    {
        try
        {
            if (File.Exists(physicalPath))
            {
                File.Delete(
                    physicalPath);
            }
        }
        catch
        {
            /*
             * Ana hatayı gizlememek için temizlik sırasında
             * oluşabilecek ikincil hata burada yutulur.
             *
             * İlerleyen aşamada bu servis içine ILogger ekleyerek
             * temizlik hatasını loglayabiliriz.
             */
        }
    }
}