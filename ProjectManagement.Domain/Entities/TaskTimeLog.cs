namespace ProjectManagement.Domain.Entities;

public class TaskTimeLog
{

    public int Id { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public DateTime WorkDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ProjectTask Task { get; set; } = null!;
    public User User { get; set; } = null!;
}