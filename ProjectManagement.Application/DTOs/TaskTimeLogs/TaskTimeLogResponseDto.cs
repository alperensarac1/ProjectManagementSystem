namespace ProjectManagement.Application.DTOs.TaskTimeLogs;

public sealed class TaskTimeLogResponseDto
{
    public int Id { get; init; }

    public int TaskId { get; init; }
    public int UserId { get; init; }
    public string UserFullName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public decimal Hours { get; init; }
    public string? Description { get; init; }
    public DateTime WorkDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }
}