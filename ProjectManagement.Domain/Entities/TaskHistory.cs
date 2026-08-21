using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Domain.Entities;

public class TaskHistory
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int ChangedByUserId { get; set; }
    public TaskChangeType ChangeType { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ProjectTask Task { get; set; } = null!;
    public User ChangedByUser { get; set; } = null!;
}