namespace ProjectManagement.Api.IntegrationTests.Models;


public sealed class RecentTaskModel
{
    public int Id { get; set; }

    public string Title { get; set; } =
        string.Empty;

    public int ProjectId { get; set; }

    public string ProjectName { get; set; } =
        string.Empty;

    public string Status { get; set; } =
        string.Empty;

    public string Priority { get; set; } =
        string.Empty;

    public int? AssignedToUserId { get; set; }

    public string? AssignedToUserFullName { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsOverdue { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}