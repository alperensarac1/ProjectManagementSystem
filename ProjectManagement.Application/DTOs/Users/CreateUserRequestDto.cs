using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.DTOs.Users;

public sealed class CreateUserRequestDto
{

    public string FirstName { get; set; } = string.Empty;

 
    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;


    public string Password { get; set; } = string.Empty;


    public UserRole Role { get; set; }

    public string? Department { get; set; }

    public bool IsActive { get; set; } = true;
}