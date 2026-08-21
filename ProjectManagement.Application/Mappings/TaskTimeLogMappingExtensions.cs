using ProjectManagement.Application.DTOs.TaskTimeLogs;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Mappings;

public static class TaskTimeLogMappingExtensions
{
    public static TaskTimeLogResponseDto ToResponseDto(
        this TaskTimeLog timeLog,
        int currentUserId,
        UserRole currentUserRole,
        int projectOwnerId)
    {
        ArgumentNullException.ThrowIfNull(timeLog);

        var isOwner = timeLog.UserId == currentUserId;

        var canDelete =
            isOwner ||
            currentUserRole == UserRole.Admin ||
            projectOwnerId == currentUserId;

        return new TaskTimeLogResponseDto
        {
            Id = timeLog.Id,
            TaskId = timeLog.TaskId,
            UserId = timeLog.UserId,

            UserFullName = timeLog.User is null
                ? string.Empty
                : $"{timeLog.User.FirstName} {timeLog.User.LastName}".Trim(),

            UserEmail = timeLog.User?.Email ?? string.Empty,

            Hours = timeLog.Hours,
            Description = timeLog.Description,
            WorkDate = timeLog.WorkDate,
            CreatedAt = timeLog.CreatedAt,

       
            CanEdit = isOwner,

            CanDelete = canDelete
        };
    }

    public static TaskTimeLogSummaryDto ToSummaryDto(
        this ProjectTask task,
        IReadOnlyCollection<TaskTimeLog> timeLogs)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(timeLogs);

        var actualHours = timeLogs.Sum(timeLog => timeLog.Hours);

        return new TaskTimeLogSummaryDto
        {
            TaskId = task.Id,
            TaskTitle = task.Title,
            EstimatedHours = task.EstimatedHours,
            ActualHours = actualHours,

            DifferenceHours = task.EstimatedHours.HasValue
                ? actualHours - task.EstimatedHours.Value
                : null,

            TimeLogCount = timeLogs.Count,

            ContributorCount = timeLogs
                .Select(timeLog => timeLog.UserId)
                .Distinct()
                .Count(),

            LastWorkDate = timeLogs.Count == 0
                ? null
                : timeLogs.Max(timeLog => timeLog.WorkDate)
        };
    }
}