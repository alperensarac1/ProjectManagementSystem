using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Interfaces.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    Task<Project?> GetByIdForUpdateAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameExceptProjectAsync(
        string name,
        int excludedProjectId,
        CancellationToken cancellationToken = default);

    Task<bool> IsUserActiveMemberAsync(
        int projectId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<(
        IReadOnlyCollection<Project> Items,
        int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            ProjectStatus? status,
            bool? isArchived,
            int? ownerId,
            int currentUserId,
            UserRole currentUserRole,
            CancellationToken cancellationToken = default);

    Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default);

    void Update(Project project);

    void Remove(Project project);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}