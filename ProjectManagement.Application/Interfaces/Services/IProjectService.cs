using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Projects;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Interfaces.Services;

public interface IProjectService
{
    Task<PagedResult<ProjectResponseDto>> GetPagedAsync(
        ProjectListQueryDto query,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<ProjectResponseDto> GetByIdAsync(
        int projectId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<ProjectResponseDto> CreateAsync(
        CreateProjectRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<ProjectResponseDto> UpdateAsync(
        int projectId,
        UpdateProjectRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<ProjectResponseDto> UpdateArchiveStatusAsync(
        int projectId,
        UpdateProjectArchiveRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int projectId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);
}