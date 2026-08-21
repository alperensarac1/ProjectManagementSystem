namespace ProjectManagement.Api.IntegrationTests.Models;

public sealed class TaskTimeLogSummaryModel
{
    public int TaskId { get; set; }

    public string TaskTitle { get; set; } =
        string.Empty;

    public decimal? EstimatedHours { get; set; }

    public decimal ActualHours { get; set; }

    public decimal? DifferenceHours { get; set; }

    public int TimeLogCount { get; set; }

    public int ContributorCount { get; set; }

    public DateTime? LastWorkDate { get; set; }
}