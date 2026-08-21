using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.DTOs.Projects;


public sealed class UpdateProjectRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public ProjectStatus Status { get; set; }

    public int? OwnerId { get; set; }
}