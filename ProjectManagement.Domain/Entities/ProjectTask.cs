using ProjectManagement.Domain.Common;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Domain.Entities;


public class ProjectTask : BaseEntity
{

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

  
    public int ProjectId { get; set; }
    public int? AssignedToUserId { get; set; }
    public int CreatedByUserId { get; set; }
    public ProjectTaskStatus Status { get; set; } =
        ProjectTaskStatus.Todo;
    public TaskPriority Priority { get; set; } =
        TaskPriority.Medium;

    public DateTime? DueDate { get; set; }

    public decimal? EstimatedHours { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Project Project { get; set; } = null!;
    public User? AssignedToUser { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public ICollection<Comment> Comments { get; set; } =
        new List<Comment>();
    public ICollection<TaskHistory> Histories { get; set; } =
        new List<TaskHistory>();
    public ICollection<TaskTimeLog> TimeLogs { get; set; } =
        new List<TaskTimeLog>();
}