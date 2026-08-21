using ProjectManagement.Application.Common.Identity;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Interfaces.Authentication;

public interface ICurrentUserService
{

    bool IsAuthenticated { get; }

    int GetUserId();

    UserRole GetUserRole();

    string? GetEmail();

    string? GetFullName();

    CurrentUserInfo GetCurrentUser();
}