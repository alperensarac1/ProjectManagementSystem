using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Services;

namespace ProjectManagement.Application.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(ApplicationServiceRegistration).Assembly);

        services.AddScoped<
            IAuthService,
            AuthService>();

        services.AddScoped<
            IUserService,
            UserService>();

        services.AddScoped<
            IProjectService,
            ProjectService>();

        services.AddScoped<
            IProjectMemberService,
            ProjectMemberService>();

        services.AddScoped<
            IProjectTaskService,
            ProjectTaskService>();

        services.AddScoped<
            ICommentService,
            CommentService>();

        services.AddScoped<
            ITaskHistoryService,
            TaskHistoryService>();

        services.AddScoped<
            ITaskTimeLogService,
            TaskTimeLogService>();

        services.AddScoped<
            IDashboardService,
            DashboardService>();
        
        services.AddScoped<
            IMailboxService,
            MailboxService>();

        services.AddScoped<
            IMailboxAttachmentCleanupService,
            MailboxAttachmentCleanupService>();

        return services;
    }
}