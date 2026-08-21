using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Domain.Entities;

public class ProjectMember
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int UserId { get; set; }
    public ProjectMemberRole Role { get; set; } =
        ProjectMemberRole.Member;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}