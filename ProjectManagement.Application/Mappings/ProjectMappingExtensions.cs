using ProjectManagement.Application.DTOs.Projects;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Mappings;

public static class ProjectMappingExtensions
{
    public static ProjectResponseDto ToResponseDto(
        this Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Status = project.Status.ToString(),

            OwnerId = project.OwnerId,

            OwnerFullName = project.Owner is null
                ? string.Empty
                : $"{project.Owner.FirstName} {project.Owner.LastName}"
                    .Trim(),

            OwnerEmail = project.Owner?.Email ?? string.Empty,

            IsArchived = project.IsArchived,
            ArchivedAt = project.ArchivedAt,

            MemberCount = project.Members.Count(member =>
                member.IsActive),

            TaskCount = project.Tasks.Count(task =>
                !task.IsDeleted),

            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
}