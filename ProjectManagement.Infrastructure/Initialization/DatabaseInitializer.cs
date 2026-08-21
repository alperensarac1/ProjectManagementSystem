using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;
using ProjectManagement.Infrastructure.Data;

namespace ProjectManagement.Infrastructure.Initialization;

public sealed class DatabaseInitializer : IDatabaseInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly InitialAdminSettings _adminSettings;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IOptions<InitialAdminSettings> adminOptions,
        ILogger<DatabaseInitializer> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _adminSettings = adminOptions.Value;
        _logger = logger;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {

        await ApplyMigrationsAsync(cancellationToken);

        if (!_adminSettings.Enabled)
        {
            _logger.LogInformation(
                "Başlangıç Admin kullanıcısı oluşturma işlemi kapalı.");

            return;
        }

        ValidateInitialAdminSettings();

        await SeedInitialAdminAsync(cancellationToken);
    }


    private async Task ApplyMigrationsAsync(
        CancellationToken cancellationToken)
    {
        var pendingMigrations =
            await _dbContext.Database
                .GetPendingMigrationsAsync(cancellationToken);

        var migrations = pendingMigrations.ToArray();

        if (migrations.Length == 0)
        {
            _logger.LogInformation(
                "Uygulanmayı bekleyen veritabanı migration işlemi bulunamadı.");

            return;
        }

        _logger.LogInformation(
            "{MigrationCount} adet migration uygulanıyor: {Migrations}",
            migrations.Length,
            string.Join(", ", migrations));

        await _dbContext.Database.MigrateAsync(
            cancellationToken);

        _logger.LogInformation(
            "Veritabanı migration işlemleri başarıyla tamamlandı.");
    }


    private async Task SeedInitialAdminAsync(
        CancellationToken cancellationToken)
    {
        var normalizedEmail =
            NormalizeEmail(_adminSettings.Email);

        var existingUser =
            await _dbContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    user => user.Email == normalizedEmail,
                    cancellationToken);

        if (existingUser is not null)
        {
            await HandleExistingUserAsync(
                existingUser,
                cancellationToken);

            return;
        }

        var adminUser = new User
        {
            FirstName = _adminSettings.FirstName.Trim(),
            LastName = _adminSettings.LastName.Trim(),
            Email = normalizedEmail,

            PasswordHash =
                _passwordHasher.Hash(_adminSettings.Password),

            Role = UserRole.Admin,
            Department =
                NormalizeOptionalText(_adminSettings.Department),

            IsActive = true,
            IsDeleted = false
        };

        await _dbContext.Users.AddAsync(
            adminUser,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Başlangıç Admin kullanıcısı oluşturuldu. Email: {Email}",
            normalizedEmail);
    }



    private async Task HandleExistingUserAsync(
        User existingUser,
        CancellationToken cancellationToken)
    {
        if (existingUser.Role == UserRole.Admin &&
            existingUser.IsActive &&
            !existingUser.IsDeleted)
        {
            _logger.LogInformation(
                "Başlangıç Admin kullanıcısı zaten mevcut. Email: {Email}",
                existingUser.Email);

            return;
        }

  
        if (!existingUser.IsDeleted &&
            existingUser.Role != UserRole.Admin)
        {
            throw new InvalidOperationException(
                $"InitialAdmin e-posta adresi '{existingUser.Email}' " +
                "başka bir kullanıcı tarafından kullanılmaktadır.");
        }

  
        if (existingUser.IsDeleted)
        {
            throw new InvalidOperationException(
                $"InitialAdmin e-posta adresi '{existingUser.Email}' " +
                "soft-delete edilmiş bir kullanıcıya aittir. " +
                "Kullanıcıyı geri yükleyin veya farklı bir e-posta kullanın.");
        }

        if (!existingUser.IsActive)
        {
            _logger.LogWarning(
                "Başlangıç Admin kullanıcısı mevcut fakat pasif. Email: {Email}",
                existingUser.Email);

            return;
        }

        await Task.CompletedTask;
    }

    private void ValidateInitialAdminSettings()
    {
        if (string.IsNullOrWhiteSpace(_adminSettings.FirstName))
        {
            throw new InvalidOperationException(
                "InitialAdmin:FirstName ayarı zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(_adminSettings.LastName))
        {
            throw new InvalidOperationException(
                "InitialAdmin:LastName ayarı zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(_adminSettings.Email))
        {
            throw new InvalidOperationException(
                "InitialAdmin:Email ayarı zorunludur.");
        }

        if (!IsValidEmail(_adminSettings.Email))
        {
            throw new InvalidOperationException(
                "InitialAdmin:Email geçerli bir e-posta adresi değildir.");
        }

        if (string.IsNullOrWhiteSpace(_adminSettings.Password))
        {
            throw new InvalidOperationException(
                "InitialAdmin:Password ayarı zorunludur.");
        }

        if (_adminSettings.Password.Length < 8)
        {
            throw new InvalidOperationException(
                "InitialAdmin:Password en az 8 karakter olmalıdır.");
        }

        if (!_adminSettings.Password.Any(char.IsUpper))
        {
            throw new InvalidOperationException(
                "InitialAdmin:Password en az bir büyük harf içermelidir.");
        }

        if (!_adminSettings.Password.Any(char.IsLower))
        {
            throw new InvalidOperationException(
                "InitialAdmin:Password en az bir küçük harf içermelidir.");
        }

        if (!_adminSettings.Password.Any(char.IsDigit))
        {
            throw new InvalidOperationException(
                "InitialAdmin:Password en az bir rakam içermelidir.");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var mailAddress =
                new System.Net.Mail.MailAddress(email.Trim());

            return string.Equals(
                mailAddress.Address,
                email.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}