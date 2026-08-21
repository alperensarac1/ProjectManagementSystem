using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.DTOs.ProjectMembers;


public sealed class UpdateProjectMemberRequestDto
{
  
    public ProjectMemberRole Role { get; set; }
}