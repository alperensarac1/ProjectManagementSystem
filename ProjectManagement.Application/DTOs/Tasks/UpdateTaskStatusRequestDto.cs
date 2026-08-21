using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.DTOs.Tasks;

public sealed class UpdateTaskStatusRequestDto
{
    public ProjectTaskStatus Status { get; set; }
}