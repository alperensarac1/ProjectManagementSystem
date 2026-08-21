using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.DTOs.Tasks;

public sealed class CreateTaskRequestDto
{

    public int ProjectId { get; set; }


    public string Title { get; set; } = string.Empty;


    public string? Description { get; set; }

 
    public int? AssignedToUserId { get; set; }


    public ProjectTaskStatus Status { get; set; } =
        ProjectTaskStatus.Todo;

    public TaskPriority Priority { get; set; } =
        TaskPriority.Medium;

    public DateTime? DueDate { get; set; }

    public decimal? EstimatedHours { get; set; }
}