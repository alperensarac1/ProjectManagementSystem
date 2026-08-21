namespace ProjectManagement.Application.DTOs.ProjectMembers;

public sealed class ProjectMemberResponseDto
{
    public int Id { get; init; }
    public int ProjectId { get; init; }
    public int UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string SystemRole { get; init; } = string.Empty;
    public string ProjectRole { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    public bool IsActive { get; init; }
    public bool IsProjectOwner { get; init; }
}