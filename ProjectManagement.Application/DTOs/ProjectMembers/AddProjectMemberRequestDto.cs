using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.DTOs.ProjectMembers;

public sealed class AddProjectMemberRequestDto
{
    public int UserId { get; set; }
    public ProjectMemberRole Role { get; set; } =
        ProjectMemberRole.Member;
}