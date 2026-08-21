using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Infrastructure.Authentication;
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(
        IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;

        ValidateSettings();
    }

    public JwtTokenResult GenerateAccessToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var nowUtc = DateTime.UtcNow;

        var expiresAtUtc = nowUtc.AddMinutes(
            _jwtSettings.ExpirationMinutes);


        var claims = new List<Claim>
        {
     
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

  
            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

    
            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

    
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

      
            new(
                ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}".Trim()),

    
            new(
                ClaimTypes.Role,
                user.Role.ToString()),
            
            new Claim(
                "token_version",
                user.TokenVersion.ToString(
                CultureInfo.InvariantCulture))
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = nowUtc,
            IssuedAt = nowUtc,
            Expires = expiresAtUtc,
            SigningCredentials = signingCredentials
        };


        var tokenHandler = new JsonWebTokenHandler();

        var accessToken = tokenHandler.CreateToken(tokenDescriptor);

        return new JwtTokenResult
        {
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer))
        {
            throw new InvalidOperationException(
                "Jwt:Issuer ayarı bulunamadı.");
        }

        if (string.IsNullOrWhiteSpace(_jwtSettings.Audience))
        {
            throw new InvalidOperationException(
                "Jwt:Audience ayarı bulunamadı.");
        }

        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
        {
            throw new InvalidOperationException(
                "Jwt:SecretKey ayarı bulunamadı.");
        }

        if (_jwtSettings.SecretKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SecretKey en az 32 karakter olmalıdır.");
        }

        if (_jwtSettings.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "Jwt:ExpirationMinutes sıfırdan büyük olmalıdır.");
        }
    }
}