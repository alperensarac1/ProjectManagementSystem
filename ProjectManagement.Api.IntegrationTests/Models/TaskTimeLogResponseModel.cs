namespace ProjectManagement.Api.IntegrationTests.Models;

public sealed class TaskTimeLogResponseModel
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public string UserFullName { get; set; } =
        string.Empty;

    public string UserEmail { get; set; } =
        string.Empty;

    public decimal Hours { get; set; }

    public string? Description { get; set; }

    public DateTime WorkDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }
}