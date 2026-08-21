namespace ProjectManagement.Application.DTOs.Users;


public class UserResponseDto
{
    public int Id { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;


    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string? Department { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }
}