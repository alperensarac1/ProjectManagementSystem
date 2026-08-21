using ProjectManagement.Application.DTOs.TaskTimeLogs;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Interfaces.Services;

public interface ITaskTimeLogService
{
    Task<IReadOnlyCollection<TaskTimeLogResponseDto>> GetByTaskIdAsync(
        int taskId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<TaskTimeLogSummaryDto> GetSummaryAsync(
        int taskId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<TaskTimeLogResponseDto> CreateAsync(
        int taskId,
        CreateTaskTimeLogRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<TaskTimeLogResponseDto> UpdateAsync(
        int taskId,
        int timeLogId,
        UpdateTaskTimeLogRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int taskId,
        int timeLogId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);
}