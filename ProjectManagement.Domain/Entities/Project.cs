using ProjectManagement.Domain.Common;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Domain.Entities;

public class Project : BaseEntity
{

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public int OwnerId { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }

    public User Owner { get; set; } = null!;
    public ICollection<ProjectTask> Tasks { get; set; } =
        new List<ProjectTask>();

    public ICollection<ProjectMember> Members { get; set; } =
        new List<ProjectMember>();
}