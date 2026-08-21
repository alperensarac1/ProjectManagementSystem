using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Infrastructure.Storage;

namespace ProjectManagement.Infrastructure.BackgroundServices;

public sealed class MailboxAttachmentCleanupHostedService
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly IOptionsMonitor<MailboxStorageSettings>
        _settingsMonitor;

    private readonly ILogger<
        MailboxAttachmentCleanupHostedService> _logger;

    public MailboxAttachmentCleanupHostedService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<MailboxStorageSettings> settingsMonitor,
        ILogger<MailboxAttachmentCleanupHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _settingsMonitor = settingsMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Mailbox dosya temizleme arka plan servisi başlatıldı.");
        
        await ExecuteCleanupSafelyAsync(
            stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var settings =
                _settingsMonitor.CurrentValue;

            if (!settings.CleanupEnabled)
            {
                _logger.LogDebug(
                    "Mailbox dosya temizleme işlemi ayarlardan kapatılmış.");

                await DelaySafelyAsync(
                    TimeSpan.FromHours(1),
                    stoppingToken);

                continue;
            }

            var intervalHours =
                settings.CleanupIntervalHours > 0
                    ? settings.CleanupIntervalHours
                    : 6;

            await DelaySafelyAsync(
                TimeSpan.FromHours(intervalHours),
                stoppingToken);

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await ExecuteCleanupSafelyAsync(
                stoppingToken);
        }

        _logger.LogInformation(
            "Mailbox dosya temizleme arka plan servisi durduruldu.");
    }

    private async Task ExecuteCleanupSafelyAsync(
        CancellationToken cancellationToken)
    {
        var settings =
            _settingsMonitor.CurrentValue;

        if (!settings.CleanupEnabled)
        {
            return;
        }

        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var cleanupService =
                scope.ServiceProvider
                    .GetRequiredService<
                        IMailboxAttachmentCleanupService>();

            var deletedFileCount =
                await cleanupService.DeleteExpiredFilesAsync(
                    cancellationToken);

            if (deletedFileCount > 0)
            {
                _logger.LogInformation(
                    "{DeletedFileCount} adet süresi dolmuş mailbox dosyası temizlendi.",
                    deletedFileCount);
            }
            else
            {
                _logger.LogDebug(
                    "Temizlenecek süresi dolmuş mailbox dosyası bulunamadı.");
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Süresi dolmuş mailbox dosyaları temizlenirken hata oluştu.");
        }
    }

    private static async Task DelaySafelyAsync(
        TimeSpan delay,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(
                delay,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}