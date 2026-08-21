using System.Security.Cryptography;
using System.Text;
using ProjectManagement.Application.Interfaces.Authentication;

namespace ProjectManagement.Infrastructure.Authentication;


public sealed class RefreshTokenGenerator
    : IRefreshTokenGenerator
{
    public string GenerateToken()
    {

        var randomBytes =
            RandomNumberGenerator.GetBytes(64);

        return Convert
            .ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public string ComputeHash(
        string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            token);

        var tokenBytes =
            Encoding.UTF8.GetBytes(token);

        var hashBytes =
            SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}