using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.DTOs.Dashboard;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Mappings;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IUserRepository _userRepository;

    public DashboardService(
        IDashboardRepository dashboardRepository,
        IUserRepository userRepository)
    {
        _dashboardRepository = dashboardRepository;
        _userRepository = userRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        await EnsureActiveUserAsync(
            currentUserId,
            cancellationToken);

        var readModel =
            await _dashboardRepository.GetSummaryAsync(
                currentUserId,
                currentUserRole,
                cancellationToken);

        return readModel.ToResponseDto();
    }

    public async Task<IReadOnlyCollection<RecentTaskDto>>
        GetRecentTasksAsync(
            int currentUserId,
            UserRole currentUserRole,
            int count,
            CancellationToken cancellationToken = default)
    {
        await EnsureActiveUserAsync(
            currentUserId,
            cancellationToken);

 
        if (count is < 1 or > 50)
        {
            throw new BusinessRuleException(
                "Görev sayısı 1 ile 50 arasında olmalıdır.");
        }

        var tasks =
            await _dashboardRepository.GetRecentTasksAsync(
                currentUserId,
                currentUserRole,
                count,
                cancellationToken);

        return tasks
            .Select(task => task.ToRecentTaskDto())
            .ToArray();
    }

    private async Task EnsureActiveUserAsync(
        int currentUserId,
        CancellationToken cancellationToken)
    {
        var user =
            await _userRepository.GetByIdAsync(
                currentUserId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "Kullanıcı bulunamadı.");
        }

        if (!user.IsActive)
        {
            throw new BusinessRuleException(
                "Kullanıcı hesabı aktif değildir.");
        }
    }
}