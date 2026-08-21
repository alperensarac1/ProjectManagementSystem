using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Storage;
using ProjectManagement.Infrastructure.Authentication;
using ProjectManagement.Infrastructure.Backup;
using ProjectManagement.Infrastructure.Data;
using ProjectManagement.Infrastructure.Data.Repositories;
using ProjectManagement.Infrastructure.Initialization;
using ProjectManagement.Infrastructure.Storage;

namespace ProjectManagement.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "DefaultConnection bağlantı bilgisi bulunamadı.");
        }

        services.AddDbContext<ApplicationDbContext>(
            options =>
            {
                options.UseSqlite(
                    connectionString);
            });

        services.Configure<JwtSettings>(
            configuration.GetSection(
                JwtSettings.SectionName));

        services.Configure<InitialAdminSettings>(
            configuration.GetSection(
                InitialAdminSettings.SectionName));

        services.Configure<RefreshTokenSettings>(
            configuration.GetSection(
                RefreshTokenSettings.SectionName));

        services.Configure<DatabaseBackupSettings>(
            configuration.GetSection(
                DatabaseBackupSettings.SectionName));

        /*
         * Mailbox fiziksel dosya depolama ayarları.
         */
        services.Configure<MailboxStorageSettings>(
            configuration.GetSection(
                MailboxStorageSettings.SectionName));

        services.AddScoped<
            IUserRepository,
            UserRepository>();

        services.AddScoped<
            IProjectRepository,
            ProjectRepository>();

        services.AddScoped<
            IProjectMemberRepository,
            ProjectMemberRepository>();

        services.AddScoped<
            IProjectTaskRepository,
            ProjectTaskRepository>();

        services.AddScoped<
            IRefreshTokenRepository,
            RefreshTokenRepository>();

        services.AddScoped<
            ICommentRepository,
            CommentRepository>();

        services.AddScoped<
            ITaskHistoryRepository,
            TaskHistoryRepository>();

        services.AddScoped<
            ITaskTimeLogRepository,
            TaskTimeLogRepository>();

        services.AddScoped<
            IDashboardRepository,
            DashboardRepository>();

        /*
         * Mailbox repository.
         */
        services.AddScoped<
            IMailboxRepository,
            MailboxRepository>();

        /*
         * Mailbox dosyalarını yerel diske kaydeden servis.
         */
        services.AddScoped<
            IMailboxFileStorage,
            LocalMailboxFileStorage>();

        services.AddScoped<
            IPasswordHasher,
            BcryptPasswordHasher>();

        services.AddScoped<
            IJwtTokenService,
            JwtTokenService>();

        services.AddSingleton<
            IRefreshTokenGenerator,
            RefreshTokenGenerator>();

        services.AddScoped<
            IDatabaseInitializer,
            DatabaseInitializer>();

        services.AddHostedService<
            DatabaseBackupHostedService>();

        return services;
    }
}