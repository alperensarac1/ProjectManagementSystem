using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.DTOs.Tasks;


public sealed class UpdateTaskRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? AssignedToUserId { get; set; }

    public ProjectTaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public decimal? EstimatedHours { get; set; }
}