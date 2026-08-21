using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Interfaces.Repositories;

public interface IProjectTaskRepository
{
    Task<ProjectTask?> GetByIdAsync(
        int taskId,
        CancellationToken cancellationToken = default);

    Task<ProjectTask?> GetByIdForUpdateAsync(
        int taskId,
        CancellationToken cancellationToken = default);

    Task<(
        IReadOnlyCollection<ProjectTask> Items,
        int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            int? projectId,
            int? assignedToUserId,
            ProjectTaskStatus? status,
            TaskPriority? priority,
            bool? isOverdue,
            int currentUserId,
            UserRole currentUserRole,
            CancellationToken cancellationToken = default);

    Task AddAsync(
        ProjectTask task,
        CancellationToken cancellationToken = default);

    void Update(ProjectTask task);

    void Remove(ProjectTask task);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}