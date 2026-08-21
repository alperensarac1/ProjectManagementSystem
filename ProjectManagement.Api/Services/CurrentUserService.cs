using System.Security.Claims;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Identity;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public int GetUserId()
    {
        EnsureAuthenticated();

        var userIdText =
            User?.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdText, out var userId) ||
            userId <= 0)
        {
            throw new UnauthorizedAccessAppException(
                "Token içerisindeki kullanıcı ID bilgisi geçersizdir.");
        }

        return userId;
    }

    public UserRole GetUserRole()
    {
        EnsureAuthenticated();

        var roleText =
            User?.FindFirstValue(
                ClaimTypes.Role);

        if (!Enum.TryParse<UserRole>(
                roleText,
                ignoreCase: true,
                out var userRole))
        {
            throw new UnauthorizedAccessAppException(
                "Token içerisindeki kullanıcı rolü geçersizdir.");
        }

        return userRole;
    }

    public string? GetEmail()
    {
        EnsureAuthenticated();

        return User?.FindFirstValue(
                   ClaimTypes.Email)
               ??
               User?.FindFirstValue("email");
    }

    public string? GetFullName()
    {
        EnsureAuthenticated();

        return User?.FindFirstValue(
            ClaimTypes.Name);
    }

    public CurrentUserInfo GetCurrentUser()
    {
        return new CurrentUserInfo
        {
            UserId = GetUserId(),
            Role = GetUserRole(),
            Email = GetEmail(),
            FullName = GetFullName(),
            IsAuthenticated = true
        };
    }

    private void EnsureAuthenticated()
    {
        if (!IsAuthenticated)
        {
            throw new UnauthorizedAccessAppException(
                "Bu işlem için giriş yapmanız gerekmektedir.");
        }
    }
}