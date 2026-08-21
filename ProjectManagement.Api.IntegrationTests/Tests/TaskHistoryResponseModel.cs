namespace ProjectManagement.Api.IntegrationTests.Models;


public sealed class TaskHistoryResponseModel
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int ChangedByUserId { get; set; }

    public string ChangedByUserFullName { get; set; } =
        string.Empty;

    public string ChangedByUserEmail { get; set; } =
        string.Empty;

    public string ChangeType { get; set; } =
        string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}