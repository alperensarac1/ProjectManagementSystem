using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Common.Identity;


public sealed class CurrentUserInfo
{

    public int UserId { get; init; }

 
    public UserRole Role { get; init; }

    public string? Email { get; init; }

    public string? FullName { get; init; }

    public bool IsAuthenticated { get; init; }
}