namespace ProjectManagement.Api.IntegrationTests.Models;

public sealed class DashboardSummaryModel
{
    public int TotalProjectCount { get; set; }

    public int ActiveProjectCount { get; set; }

    public int PlanningProjectCount { get; set; }

    public int CompletedProjectCount { get; set; }

    public int ArchivedProjectCount { get; set; }

    public int TotalTaskCount { get; set; }

    public int TodoTaskCount { get; set; }

    public int InProgressTaskCount { get; set; }

    public int InReviewTaskCount { get; set; }

    public int DoneTaskCount { get; set; }

    public int OverdueTaskCount { get; set; }

    public int MyAssignedTaskCount { get; set; }

    public int MyOverdueTaskCount { get; set; }

    public decimal TotalEstimatedHours { get; set; }

    public decimal TotalActualHours { get; set; }

    public decimal MyLoggedHours { get; set; }

    public decimal TaskCompletionPercentage { get; set; }

    public decimal TimeUsagePercentage { get; set; }

    public DateTime GeneratedAtUtc { get; set; }
}