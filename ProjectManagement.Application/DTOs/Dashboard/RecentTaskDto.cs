namespace ProjectManagement.Application.DTOs.Dashboard;

public sealed class RecentTaskDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public int? AssignedToUserId { get; init; }
    public string? AssignedToUserFullName { get; init; }
    public DateTime? DueDate { get; init; }
    public bool IsOverdue { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}