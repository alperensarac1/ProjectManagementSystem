namespace ProjectManagement.Api.IntegrationTests.Models;

public sealed class AuthResponseModel
{
    public string AccessToken { get; set; } =
        string.Empty;

    public string RefreshToken { get; set; } =
        string.Empty;


    public string TokenType { get; set; } =
        "Bearer";

    public DateTime ExpiresAtUtc { get; set; }

    public UserResponseModel User { get; set; } =
        null!;
}