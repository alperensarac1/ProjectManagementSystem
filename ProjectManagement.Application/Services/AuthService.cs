using Microsoft.Extensions.Options;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Settings;
using ProjectManagement.Application.DTOs.Auth;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Mappings;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly RefreshTokenSettings _refreshTokenSettings;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenGenerator refreshTokenGenerator,
        IOptions<RefreshTokenSettings> refreshTokenOptions)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenSettings = refreshTokenOptions.Value;
    }

   
    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedEmail =
            NormalizeEmail(request.Email);

     
        var emailExists =
            await _userRepository.ExistsByEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(
                "Bu e-posta adresiyle kayıtlı bir kullanıcı bulunmaktadır.");
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,

            PasswordHash =
                _passwordHasher.Hash(request.Password),

            Role = UserRole.TeamMember,

            Department =
                NormalizeOptionalText(request.Department),

            IsActive = true,

            TokenVersion = 0
        };

        await _userRepository.AddAsync(
            user,
            cancellationToken);

      
    
        await _userRepository.SaveChangesAsync(
            cancellationToken);

        return await CreateAuthenticationResponseAsync(
            user,
            ipAddress: null,
            cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedEmail =
            NormalizeEmail(request.Email);

        var user =
            await _userRepository.GetByEmailAsync(
                normalizedEmail,
                cancellationToken);
        if (user is null ||
            !_passwordHasher.Verify(
                request.Password,
                user.PasswordHash))
        {
            throw new AuthenticationFailedException(
                "E-posta adresi veya şifre hatalıdır.");
        }

        if (!user.IsActive)
        {
            throw new AuthenticationFailedException(
                "Bu kullanıcı hesabı aktif değildir.");
        }

        return await CreateAuthenticationResponseAsync(
            user,
            ipAddress: null,
            cancellationToken);
    }


    public async Task ChangePasswordAsync(
        int currentUserId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user =
            await _userRepository.GetByIdForUpdateAsync(
                currentUserId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "Kullanıcı bulunamadı.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException(
                "Pasif kullanıcı hesabının şifresi değiştirilemez.");
        }

        var currentPasswordIsValid =
            _passwordHasher.Verify(
                request.CurrentPassword,
                user.PasswordHash);

        if (!currentPasswordIsValid)
        {
            throw new AuthenticationFailedException(
                "Mevcut şifre doğru değildir.");
        }

        var newPasswordMatchesCurrentPassword =
            _passwordHasher.Verify(
                request.NewPassword,
                user.PasswordHash);

        if (newPasswordMatchesCurrentPassword)
        {
            throw new BusinessRuleException(
                "Yeni şifre mevcut şifreyle aynı olamaz.");
        }

        var nowUtc = DateTime.UtcNow;

        user.PasswordHash =
            _passwordHasher.Hash(
                request.NewPassword);

        user.TokenVersion++;

        user.UpdatedAt = nowUtc;

        _userRepository.Update(user);

  
        await _refreshTokenRepository.RevokeAllActiveTokensAsync(
            user.Id,
            nowUtc,
            revokedByIp: null,
            reason: "Kullanıcı şifresini değiştirdi.",
            cancellationToken);


        await _refreshTokenRepository.SaveChangesAsync(
            cancellationToken);
    }


    public async Task<AuthResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var currentTokenHash =
            _refreshTokenGenerator.ComputeHash(
                request.RefreshToken);

        var currentRefreshToken =
            await _refreshTokenRepository
                .GetByTokenHashForUpdateAsync(
                    currentTokenHash,
                    cancellationToken);

        if (currentRefreshToken is null)
        {
            throw new AuthenticationFailedException(
                "Refresh token geçersizdir.");
        }


        if (currentRefreshToken.RevokedAtUtc.HasValue)
        {
            await HandleRefreshTokenReuseAsync(
                currentRefreshToken,
                ipAddress,
                cancellationToken);

            throw new AuthenticationFailedException(
                "Refresh token daha önce kullanılmış veya iptal edilmiştir.");
        }

        if (currentRefreshToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new AuthenticationFailedException(
                "Refresh token süresi dolmuştur.");
        }

        var user = currentRefreshToken.User;

        if (user is null || user.IsDeleted)
        {
            throw new AuthenticationFailedException(
                "Refresh token ile ilişkili kullanıcı bulunamadı.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException(
                "Kullanıcı hesabı aktif değildir.");
        }

        var nowUtc = DateTime.UtcNow;


        var newPlainRefreshToken =
            _refreshTokenGenerator.GenerateToken();

        var newRefreshTokenHash =
            _refreshTokenGenerator.ComputeHash(
                newPlainRefreshToken);

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            CreatedAtUtc = nowUtc,

            ExpiresAtUtc = nowUtc.AddDays(
                GetRefreshTokenExpirationDays()),

            CreatedByIp =
                NormalizeIpAddress(ipAddress)
        };

        currentRefreshToken.RevokedAtUtc = nowUtc;

        currentRefreshToken.RevokedByIp =
            NormalizeIpAddress(ipAddress);

        currentRefreshToken.ReplacedByTokenHash =
            newRefreshTokenHash;

        currentRefreshToken.RevocationReason =
            "Refresh token rotation işlemi gerçekleştirildi.";

        _refreshTokenRepository.Update(
            currentRefreshToken);

        await _refreshTokenRepository.AddAsync(
            newRefreshToken,
            cancellationToken);


        await RevokeExcessActiveTokensAsync(
            user.Id,
            newRefreshTokenHash,
            ipAddress,
            cancellationToken);

        await _refreshTokenRepository.SaveChangesAsync(
            cancellationToken);

        var accessTokenResult =
            _jwtTokenService.GenerateAccessToken(user);

        return new AuthResponseDto
        {
            AccessToken =
                accessTokenResult.AccessToken,

            RefreshToken =
                newPlainRefreshToken,

            ExpiresAtUtc =
                accessTokenResult.ExpiresAtUtc,

            User =
                user.ToResponseDto()
        };
    }

    public async Task LogoutAsync(
        int currentUserId,
        LogoutRequestDto request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tokenHash =
            _refreshTokenGenerator.ComputeHash(
                request.RefreshToken);

        var refreshToken =
            await _refreshTokenRepository
                .GetByTokenHashForUpdateAsync(
                    tokenHash,
                    cancellationToken);

 
        if (refreshToken is null)
        {
            return;
        }

        if (refreshToken.UserId != currentUserId)
        {
            throw new ForbiddenException(
                "Bu refresh token üzerinde işlem yapma yetkiniz bulunmamaktadır.");
        }

 
        if (refreshToken.RevokedAtUtc.HasValue)
        {
            return;
        }

        refreshToken.RevokedAtUtc =
            DateTime.UtcNow;

        refreshToken.RevokedByIp =
            NormalizeIpAddress(ipAddress);

        refreshToken.RevocationReason =
            "Kullanıcı mevcut cihazdan çıkış yaptı.";

        _refreshTokenRepository.Update(
            refreshToken);

        await _refreshTokenRepository.SaveChangesAsync(
            cancellationToken);
    }


    public async Task LogoutAllAsync(
        int currentUserId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user =
            await _userRepository.GetByIdForUpdateAsync(
                currentUserId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "Kullanıcı bulunamadı.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException(
                "Kullanıcı hesabı aktif değildir.");
        }

        var nowUtc = DateTime.UtcNow;

        await _refreshTokenRepository.RevokeAllActiveTokensAsync(
            currentUserId,
            nowUtc,
            NormalizeIpAddress(ipAddress),
            "Kullanıcı bütün cihazlardan çıkış yaptı.",
            cancellationToken);

        user.TokenVersion++;
        user.UpdatedAt = nowUtc;

        _userRepository.Update(user);

        await _refreshTokenRepository.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<AuthResponseDto>
        CreateAuthenticationResponseAsync(
            User user,
            string? ipAddress,
            CancellationToken cancellationToken)
    {
        var plainRefreshToken =
            _refreshTokenGenerator.GenerateToken();

        var refreshTokenHash =
            _refreshTokenGenerator.ComputeHash(
                plainRefreshToken);

        var nowUtc = DateTime.UtcNow;

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAtUtc = nowUtc,

            ExpiresAtUtc = nowUtc.AddDays(
                GetRefreshTokenExpirationDays()),

            CreatedByIp =
                NormalizeIpAddress(ipAddress)
        };

        await _refreshTokenRepository.AddAsync(
            refreshToken,
            cancellationToken);


        await RevokeExcessActiveTokensAsync(
            user.Id,
            refreshTokenHash,
            ipAddress,
            cancellationToken);

        await _refreshTokenRepository.SaveChangesAsync(
            cancellationToken);

        var accessTokenResult =
            _jwtTokenService.GenerateAccessToken(user);

        return new AuthResponseDto
        {
            AccessToken =
                accessTokenResult.AccessToken,

            RefreshToken =
                plainRefreshToken,

            ExpiresAtUtc =
                accessTokenResult.ExpiresAtUtc,

            User =
                user.ToResponseDto()
        };
    }


    private async Task HandleRefreshTokenReuseAsync(
        RefreshToken reusedToken,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        await _refreshTokenRepository.RevokeAllActiveTokensAsync(
            reusedToken.UserId,
            nowUtc,
            NormalizeIpAddress(ipAddress),
            "İptal edilmiş refresh token tekrar kullanılmaya çalışıldı.",
            cancellationToken);

        var user = reusedToken.User;

        if (user is not null)
        {
            user.TokenVersion++;
            user.UpdatedAt = nowUtc;

            _userRepository.Update(user);
        }

        await _refreshTokenRepository.SaveChangesAsync(
            cancellationToken);
    }

    private async Task RevokeExcessActiveTokensAsync(
        int userId,
        string newlyCreatedTokenHash,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var maximumActiveTokens =
            _refreshTokenSettings.MaximumActiveTokensPerUser;

        if (maximumActiveTokens <= 0)
        {
            return;
        }

        var activeTokens =
            await _refreshTokenRepository
                .GetActiveTokensByUserIdAsync(
                    userId,
                    cancellationToken);

        var nowUtc = DateTime.UtcNow;


        var currentlyActiveStoredTokens =
            activeTokens
                .Where(token =>
                    token.RevokedAtUtc is null &&
                    token.ExpiresAtUtc > nowUtc &&
                    token.TokenHash != newlyCreatedTokenHash)
                .OrderBy(token => token.CreatedAtUtc)
                .ToArray();

        var activeTokenCountAfterCreation =
            currentlyActiveStoredTokens.Length + 1;

        var revokeCount =
            activeTokenCountAfterCreation -
            maximumActiveTokens;

        if (revokeCount <= 0)
        {
            return;
        }

        var tokensToRevoke =
            currentlyActiveStoredTokens
                .Take(revokeCount)
                .ToArray();

        foreach (var token in tokensToRevoke)
        {
            token.RevokedAtUtc = nowUtc;

            token.RevokedByIp =
                NormalizeIpAddress(ipAddress);

            token.RevocationReason =
                "Aktif refresh token limiti aşıldığı için iptal edildi.";

            _refreshTokenRepository.Update(token);
        }
    }

    private int GetRefreshTokenExpirationDays()
    {
        return _refreshTokenSettings.ExpirationDays > 0
            ? _refreshTokenSettings.ExpirationDays
            : 14;
    }

    private static string NormalizeEmail(
        string email)
    {
        return email
            .Trim()
            .ToLowerInvariant();
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string? NormalizeIpAddress(
        string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return null;
        }

        var normalizedIp =
            ipAddress.Trim();

        return normalizedIp.Length <= 64
            ? normalizedIp
            : normalizedIp[..64];
    }
}