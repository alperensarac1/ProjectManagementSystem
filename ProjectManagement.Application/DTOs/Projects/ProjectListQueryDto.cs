using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.DTOs.Projects;

public sealed class ProjectListQueryDto
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public ProjectStatus? Status { get; set; }

    public bool? IsArchived { get; set; }

    public int? OwnerId { get; set; }
}