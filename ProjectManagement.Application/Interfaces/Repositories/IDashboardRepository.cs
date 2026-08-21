using ProjectManagement.Application.Common.ReadModels;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<DashboardSummaryReadModel> GetSummaryAsync(
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProjectTask>> GetRecentTasksAsync(
        int currentUserId,
        UserRole currentUserRole,
        int count,
        CancellationToken cancellationToken = default);
}