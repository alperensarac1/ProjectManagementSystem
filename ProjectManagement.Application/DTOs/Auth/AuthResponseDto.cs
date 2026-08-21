using ProjectManagement.Application.DTOs.Users;

namespace ProjectManagement.Application.DTOs.Auth;


public sealed class AuthResponseDto
{

    public string AccessToken { get; init; } =
        string.Empty;

    public string RefreshToken { get; init; } =
        string.Empty;

    public string TokenType { get; init; } =
        "Bearer";

    public DateTime ExpiresAtUtc { get; init; }
    public UserResponseDto User { get; init; } =
        null!;
}