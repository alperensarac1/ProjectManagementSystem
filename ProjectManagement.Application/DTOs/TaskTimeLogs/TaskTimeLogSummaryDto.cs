namespace ProjectManagement.Application.DTOs.TaskTimeLogs;


public sealed class TaskTimeLogSummaryDto
{
    public int TaskId { get; init; }
    public string TaskTitle { get; init; } = string.Empty;
    public decimal? EstimatedHours { get; init; }
    public decimal ActualHours { get; init; }
    public decimal? DifferenceHours { get; init; }
    public int TimeLogCount { get; init; }
    public int ContributorCount { get; init; }
    public DateTime? LastWorkDate { get; init; }
}