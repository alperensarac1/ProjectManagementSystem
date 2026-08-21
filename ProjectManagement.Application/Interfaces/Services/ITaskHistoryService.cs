using ProjectManagement.Application.DTOs.TaskHistories;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Interfaces.Services;

public interface ITaskHistoryService
{
    Task<IReadOnlyCollection<TaskHistoryResponseDto>> GetByTaskIdAsync(
        int taskId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);
}