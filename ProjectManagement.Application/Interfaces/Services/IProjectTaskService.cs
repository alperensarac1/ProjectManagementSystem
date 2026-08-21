using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Tasks;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Interfaces.Services;

/// <summary>
/// Görev yönetimi iş akışlarını tanımlar.
/// </summary>
public interface IProjectTaskService
{
    Task<PagedResult<TaskResponseDto>> GetPagedAsync(
        TaskListQueryDto query,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<TaskResponseDto> GetByIdAsync(
        int taskId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<TaskResponseDto> CreateAsync(
        CreateTaskRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<TaskResponseDto> UpdateAsync(
        int taskId,
        UpdateTaskRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<TaskResponseDto> UpdateStatusAsync(
        int taskId,
        UpdateTaskStatusRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<TaskResponseDto> AssignAsync(
        int taskId,
        AssignTaskRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int taskId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);
}