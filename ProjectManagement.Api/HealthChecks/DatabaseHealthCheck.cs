using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProjectManagement.Infrastructure.Data;

namespace ProjectManagement.Api.HealthChecks;


public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        ApplicationDbContext dbContext,
        ILogger<DatabaseHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect =
                await _dbContext.Database.CanConnectAsync(
                    cancellationToken);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy(
                    "SQLite veritabanına bağlantı kurulamadı.");
            }

            return HealthCheckResult.Healthy(
                "SQLite veritabanı bağlantısı başarılı.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Veritabanı health check sırasında hata oluştu.");

            return HealthCheckResult.Unhealthy(
                "Veritabanı sağlık kontrolü başarısız oldu.",
                exception);
        }
    }
}