using ProjectManagement.Application.DTOs.TaskHistories;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Mappings;

public static class TaskHistoryMappingExtensions
{
    public static TaskHistoryResponseDto ToResponseDto(
        this TaskHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);

        return new TaskHistoryResponseDto
        {
            Id = history.Id,
            TaskId = history.TaskId,
            ChangedByUserId = history.ChangedByUserId,

            ChangedByUserFullName =
                history.ChangedByUser is null
                    ? string.Empty
                    : $"{history.ChangedByUser.FirstName} " +
                      $"{history.ChangedByUser.LastName}".Trim(),

            ChangedByUserEmail =
                history.ChangedByUser?.Email ?? string.Empty,

            ChangeType = history.ChangeType.ToString(),
            OldValue = history.OldValue,
            NewValue = history.NewValue,
            Description = history.Description,
            CreatedAt = history.CreatedAt
        };
    }
}