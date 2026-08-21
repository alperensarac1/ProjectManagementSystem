using ProjectManagement.Application.Common.ReadModels;
using ProjectManagement.Application.DTOs.Dashboard;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Mappings;

public static class DashboardMappingExtensions
{
    public static DashboardSummaryDto ToResponseDto(
        this DashboardSummaryReadModel readModel)
    {
        ArgumentNullException.ThrowIfNull(readModel);

        var completionPercentage =
            readModel.TotalTaskCount == 0
                ? 0
                : Math.Round(
                    readModel.DoneTaskCount * 100M /
                    readModel.TotalTaskCount,
                    2);

        var timeUsagePercentage =
            readModel.TotalEstimatedHours <= 0
                ? 0
                : Math.Round(
                    readModel.TotalActualHours * 100M /
                    readModel.TotalEstimatedHours,
                    2);

        return new DashboardSummaryDto
        {
            TotalProjectCount =
                readModel.TotalProjectCount,

            ActiveProjectCount =
                readModel.ActiveProjectCount,

            PlanningProjectCount =
                readModel.PlanningProjectCount,

            CompletedProjectCount =
                readModel.CompletedProjectCount,

            ArchivedProjectCount =
                readModel.ArchivedProjectCount,

            TotalTaskCount =
                readModel.TotalTaskCount,

            TodoTaskCount =
                readModel.TodoTaskCount,

            InProgressTaskCount =
                readModel.InProgressTaskCount,

            InReviewTaskCount =
                readModel.InReviewTaskCount,

            DoneTaskCount =
                readModel.DoneTaskCount,

            OverdueTaskCount =
                readModel.OverdueTaskCount,

            MyAssignedTaskCount =
                readModel.MyAssignedTaskCount,

            MyOverdueTaskCount =
                readModel.MyOverdueTaskCount,

            TotalEstimatedHours =
                readModel.TotalEstimatedHours,

            TotalActualHours =
                readModel.TotalActualHours,

            MyLoggedHours =
                readModel.MyLoggedHours,

            TaskCompletionPercentage =
                completionPercentage,

            TimeUsagePercentage =
                timeUsagePercentage,

            GeneratedAtUtc = DateTime.UtcNow
        };
    }
    public static RecentTaskDto ToRecentTaskDto(
        this ProjectTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return new RecentTaskDto
        {
            Id = task.Id,
            Title = task.Title,

            ProjectId = task.ProjectId,
            ProjectName =
                task.Project?.Name ?? string.Empty,

            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),

            AssignedToUserId =
                task.AssignedToUserId,

            AssignedToUserFullName =
                task.AssignedToUser is null
                    ? null
                    : $"{task.AssignedToUser.FirstName} " +
                      $"{task.AssignedToUser.LastName}".Trim(),

            DueDate = task.DueDate,

            IsOverdue =
                task.Status != ProjectTaskStatus.Done &&
                task.DueDate.HasValue &&
                task.DueDate.Value < DateTime.UtcNow,

            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}