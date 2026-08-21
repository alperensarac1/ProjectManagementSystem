using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.DTOs.Users;

public sealed class UpdateUserRequestDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public string? Department { get; set; }
}