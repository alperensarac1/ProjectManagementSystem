using ProjectManagement.Application.DTOs.Users;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Mappings;

public static class UserMappingExtensions
{
    public static UserResponseDto ToResponseDto(this User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email,
            Role = user.Role.ToString(),
            Department = user.Department,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}