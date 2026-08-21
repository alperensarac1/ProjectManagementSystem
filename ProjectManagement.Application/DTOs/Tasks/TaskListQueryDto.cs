using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.DTOs.Tasks;

public sealed class TaskListQueryDto
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public int? ProjectId { get; set; }
    public int? AssignedToUserId { get; set; }

    public ProjectTaskStatus? Status { get; set; }

    public TaskPriority? Priority { get; set; }
    public bool? IsOverdue { get; set; }
}