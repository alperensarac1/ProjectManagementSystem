using ProjectManagement.Application.DTOs.Dashboard;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Interfaces.Services;

public interface IDashboardService
{

    Task<DashboardSummaryDto> GetSummaryAsync(
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RecentTaskDto>> GetRecentTasksAsync(
        int currentUserId,
        UserRole currentUserRole,
        int count,
        CancellationToken cancellationToken = default);
}