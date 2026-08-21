namespace ProjectManagement.Application.DTOs.Tasks;

public sealed class TaskResponseDto
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int ProjectId { get; init; }

    public string ProjectName { get; init; } = string.Empty;

    public int? AssignedToUserId { get; init; }

    public string? AssignedToUserFullName { get; init; }

    public int CreatedByUserId { get; init; }

    public string CreatedByUserFullName { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string Priority { get; init; } = string.Empty;

    public DateTime? DueDate { get; init; }

    public decimal? EstimatedHours { get; init; }

    public decimal ActualHours { get; init; }

    public DateTime? CompletedAt { get; init; }

    public bool IsOverdue { get; init; }

    public int CommentCount { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}