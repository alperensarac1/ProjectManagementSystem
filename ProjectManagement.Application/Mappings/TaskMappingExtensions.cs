using ProjectManagement.Application.DTOs.Tasks;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Mappings;

public static class TaskMappingExtensions
{
    public static TaskResponseDto ToResponseDto(
        this ProjectTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        var nowUtc = DateTime.UtcNow;

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,

            ProjectId = task.ProjectId,
            ProjectName = task.Project?.Name ?? string.Empty,

            AssignedToUserId = task.AssignedToUserId,

            AssignedToUserFullName = task.AssignedToUser is null
                ? null
                : $"{task.AssignedToUser.FirstName} " +
                  $"{task.AssignedToUser.LastName}".Trim(),

            CreatedByUserId = task.CreatedByUserId,

            CreatedByUserFullName = task.CreatedByUser is null
                ? string.Empty
                : $"{task.CreatedByUser.FirstName} " +
                  $"{task.CreatedByUser.LastName}".Trim(),

            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            DueDate = task.DueDate,
            EstimatedHours = task.EstimatedHours,

            ActualHours = task.TimeLogs.Sum(
                timeLog => timeLog.Hours),

            CompletedAt = task.CompletedAt,

            IsOverdue =
                task.Status != ProjectTaskStatus.Done &&
                task.DueDate.HasValue &&
                task.DueDate.Value < nowUtc,

            CommentCount = task.Comments.Count(
                comment => !comment.IsDeleted),

            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}