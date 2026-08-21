using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectManagement.Infrastructure.Data;
using ProjectManagement.Infrastructure.Initialization;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;


public sealed class ProjectManagementApiFactory
    : WebApplicationFactory<Program>,
      IAsyncLifetime
{
    private DbConnection? _databaseConnection;
    
    public string MailboxRootDirectory { get; } =
        Path.Combine(
            Path.GetTempPath(),
            "ProjectManagementIntegrationTests",
            Guid.NewGuid().ToString("N"),
            "mailbox");

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
            {
                var testConfiguration =
                    new Dictionary<string, string?>
                    {
                        ["MailboxStorage:RootDirectory"] =
                            MailboxRootDirectory,

                        ["MailboxStorage:RetentionMonths"] =
                            "1",
                        
                        ["MailboxStorage:CleanupEnabled"] =
                            "false",

                        ["MailboxStorage:CleanupIntervalHours"] =
                            "24"
                    };

                configurationBuilder.AddInMemoryCollection(
                    testConfiguration);
            });

        builder.ConfigureServices(
            services =>
            {
                services.RemoveAll<
                    DbContextOptions<ApplicationDbContext>>();

                services.RemoveAll<ApplicationDbContext>();
                
                services.RemoveAll<IDatabaseInitializer>();
                
                _databaseConnection =
                    new SqliteConnection(
                        "Data Source=:memory:;Foreign Keys=True");

                _databaseConnection.Open();

                services.AddSingleton<DbConnection>(
                    _databaseConnection);

                services.AddDbContext<ApplicationDbContext>(
                    (serviceProvider, options) =>
                    {
                        var connection =
                            serviceProvider
                                .GetRequiredService<DbConnection>();

                        options.UseSqlite(connection);

                        options.EnableDetailedErrors();

                        
                        options.EnableSensitiveDataLogging();
                    });

                services.AddScoped<
                    IDatabaseInitializer,
                    TestDatabaseInitializer>();
            });
    }

    public async Task InitializeAsync()
    {
       
        Directory.CreateDirectory(
            MailboxRootDirectory);

        await using var scope =
            Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
        
        await dbContext.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_databaseConnection is not null)
        {
            await _databaseConnection.DisposeAsync();

            _databaseConnection = null;
        }

        DeleteDirectorySafely(
            MailboxRootDirectory);

        await base.DisposeAsync();
    }

    private static void DeleteDirectorySafely(
        string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(
                    directoryPath,
                    recursive: true);
            }
        }
        catch
        {
        }
    }
}