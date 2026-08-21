using ProjectManagement.Application.DTOs.ProjectMembers;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Interfaces.Services;


public interface IProjectMemberService
{
    Task<IReadOnlyCollection<ProjectMemberResponseDto>> GetMembersAsync(
        int projectId,
        int currentUserId,
        UserRole currentUserRole,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<ProjectMemberResponseDto> AddMemberAsync(
        int projectId,
        AddProjectMemberRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<ProjectMemberResponseDto> UpdateMemberAsync(
        int projectId,
        int userId,
        UpdateProjectMemberRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(
        int projectId,
        int userId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);
}