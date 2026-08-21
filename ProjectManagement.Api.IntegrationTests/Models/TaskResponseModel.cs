namespace ProjectManagement.Api.IntegrationTests.Models;

public sealed class TaskResponseModel
{
    public int Id { get; set; }

    public string Title { get; set; } =
        string.Empty;

    public string? Description { get; set; }

    public int ProjectId { get; set; }

    public string ProjectName { get; set; } =
        string.Empty;

    public int? AssignedToUserId { get; set; }

    public string? AssignedToUserFullName { get; set; }

    public int CreatedByUserId { get; set; }

    public string CreatedByUserFullName { get; set; } =
        string.Empty;

    public string Status { get; set; } =
        string.Empty;

    public string Priority { get; set; } =
        string.Empty;

    public DateTime? DueDate { get; set; }

    public decimal? EstimatedHours { get; set; }

    public decimal ActualHours { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool IsOverdue { get; set; }

    public int CommentCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}