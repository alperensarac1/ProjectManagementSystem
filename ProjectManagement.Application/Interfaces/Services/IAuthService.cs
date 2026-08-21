using ProjectManagement.Application.DTOs.Auth;

namespace ProjectManagement.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        int currentUserId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AuthResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        int currentUserId,
        LogoutRequestDto request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task LogoutAllAsync(
        int currentUserId,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}