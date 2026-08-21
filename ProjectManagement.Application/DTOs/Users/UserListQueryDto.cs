using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.DTOs.Users;

public sealed class UserListQueryDto
{

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }

    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}