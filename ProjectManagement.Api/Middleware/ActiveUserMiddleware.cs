using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Api.Middleware;

public sealed class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IUserRepository userRepository)
    {

        var endpoint = context.GetEndpoint();

        var allowAnonymous =
            endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

        if (allowAnonymous)
        {
            await _next(context);
            return;
        }


        var principal = context.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var userIdText =
            principal.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdText, out var userId) ||
            userId <= 0)
        {
            throw new UnauthorizedAccessAppException(
                "Token içerisindeki kullanıcı ID bilgisi geçersizdir.");
        }

        var databaseUser =
            await userRepository.GetByIdAsync(
                userId,
                context.RequestAborted);

        if (databaseUser is null)
        {
            throw new UnauthorizedAccessAppException(
                "Token ile ilişkili kullanıcı bulunamadı veya silinmiş.");
        }
        if (databaseUser.IsDeleted)
        {
            throw new UnauthorizedAccessAppException(
                "Kullanıcı hesabı artık geçerli değildir.");
        }

       if (!databaseUser.IsActive)
{
           
            throw new UnauthorizedAccessAppException(
                "Kullanıcı hesabı pasif hâle getirilmiştir. " +
                "Lütfen sistem yöneticinizle iletişime geçiniz.");
        }

        var tokenRoleText =
            principal.FindFirstValue(
                ClaimTypes.Role);

        if (!Enum.TryParse<UserRole>(
                tokenRoleText,
                ignoreCase: true,
                out var tokenRole))
        {
            throw new UnauthorizedAccessAppException(
                "Token içerisindeki kullanıcı rolü geçersizdir.");
        }
        var tokenVersionText =
            principal.FindFirstValue(
                "token_version");

        if (!int.TryParse(
                tokenVersionText,
                out var tokenVersion))
        {
            throw new UnauthorizedAccessAppException(
                "Token sürüm bilgisi geçersizdir.");
        }

        if (tokenVersion != databaseUser.TokenVersion)
        {
            throw new UnauthorizedAccessAppException(
                "Oturumunuz geçersiz hâle getirilmiştir. " +
                "Lütfen yeniden giriş yapınız.");
        }
        if (tokenRole != databaseUser.Role)
        {
            throw new UnauthorizedAccessAppException(
                "Kullanıcı rolünüz değişmiştir. Lütfen yeniden giriş yapınız.");
        }

        await _next(context);
    }
}