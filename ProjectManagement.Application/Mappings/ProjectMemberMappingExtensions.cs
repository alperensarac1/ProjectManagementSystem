using ProjectManagement.Application.DTOs.ProjectMembers;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Mappings;
public static class ProjectMemberMappingExtensions
{

    public static ProjectMemberResponseDto ToResponseDto(
        this ProjectMember member,
        int projectOwnerId)
    {
        ArgumentNullException.ThrowIfNull(member);

        return new ProjectMemberResponseDto
        {
            Id = member.Id,
            ProjectId = member.ProjectId,
            UserId = member.UserId,

            FirstName = member.User?.FirstName ?? string.Empty,
            LastName = member.User?.LastName ?? string.Empty,

            FullName = member.User is null
                ? string.Empty
                : $"{member.User.FirstName} {member.User.LastName}".Trim(),

            Email = member.User?.Email ?? string.Empty,

            SystemRole = member.User?.Role.ToString() ?? string.Empty,

            ProjectRole = member.Role.ToString(),

            JoinedAt = member.JoinedAt,
            IsActive = member.IsActive,

            IsProjectOwner = member.UserId == projectOwnerId
        };
    }
}