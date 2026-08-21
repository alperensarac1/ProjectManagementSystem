namespace ProjectManagement.Application.Common.ReadModels;

public sealed class DashboardSummaryReadModel
{
    public int TotalProjectCount { get; init; }

    public int ActiveProjectCount { get; init; }

    public int PlanningProjectCount { get; init; }

    public int CompletedProjectCount { get; init; }

    public int ArchivedProjectCount { get; init; }

    public int TotalTaskCount { get; init; }

    public int TodoTaskCount { get; init; }

    public int InProgressTaskCount { get; init; }

    public int InReviewTaskCount { get; init; }

    public int DoneTaskCount { get; init; }

    public int OverdueTaskCount { get; init; }

    public int MyAssignedTaskCount { get; init; }

    public int MyOverdueTaskCount { get; init; }

    public decimal TotalEstimatedHours { get; init; }

    public decimal TotalActualHours { get; init; }

    public decimal MyLoggedHours { get; init; }
}