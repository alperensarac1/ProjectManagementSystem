namespace ProjectManagement.Application.DTOs.TaskHistories;

public sealed class TaskHistoryResponseDto
{
    public int Id { get; init; }

    public int TaskId { get; init; }

    public int ChangedByUserId { get; init; }

    public string ChangedByUserFullName { get; init; } = string.Empty;

    public string ChangedByUserEmail { get; init; } = string.Empty;

    public string ChangeType { get; init; } = string.Empty;

    public string? OldValue { get; init; }

    public string? NewValue { get; init; }

    public string? Description { get; init; }

    public DateTime CreatedAt { get; init; }
}