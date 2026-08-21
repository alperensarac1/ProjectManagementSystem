using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Users;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Mappings;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<PagedResult<UserResponseDto>> GetPagedAsync(
        UserListQueryDto query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var result =
            await _userRepository.GetPagedAsync(
                query.Page,
                query.PageSize,
                query.Search,
                query.Role,
                query.IsActive,
                cancellationToken);

        var users = result.Items
            .Select(user => user.ToResponseDto())
            .ToArray();

        return PagedResult<UserResponseDto>.Create(
            users,
            query.Page,
            query.PageSize,
            result.TotalCount);
    }

    public async Task<UserResponseDto> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user =
            await _userRepository.GetByIdAsync(
                userId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "Kullanıcı bulunamadı.");
        }

        return user.ToResponseDto();
    }

    public async Task<UserResponseDto> CreateAsync(
        CreateUserRequestDto request,
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
                "Bu e-posta adresi başka bir kullanıcı tarafından kullanılmaktadır.");
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,

            PasswordHash =
                _passwordHasher.Hash(
                    request.Password),

            Role = request.Role,

            Department =
                NormalizeOptionalText(
                    request.Department),

            IsActive = request.IsActive
        };

        await _userRepository.AddAsync(
            user,
            cancellationToken);

        await _userRepository.SaveChangesAsync(
            cancellationToken);

        return user.ToResponseDto();
    }

    public async Task<UserResponseDto> UpdateAsync(
        int userId,
        UpdateUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user =
            await _userRepository.GetByIdForUpdateAsync(
                userId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "Güncellenecek kullanıcı bulunamadı.");
        }

        var normalizedEmail =
            NormalizeEmail(request.Email);

        var emailUsedByAnotherUser =
            await _userRepository.ExistsByEmailExceptUserAsync(
                normalizedEmail,
                userId,
                cancellationToken);

        if (emailUsedByAnotherUser)
        {
            throw new ConflictException(
                "Bu e-posta adresi başka bir kullanıcı tarafından kullanılmaktadır.");
        }

        user.FirstName =
            request.FirstName.Trim();

        user.LastName =
            request.LastName.Trim();

        user.Email =
            normalizedEmail;

        user.Role =
            request.Role;

        user.Department =
            NormalizeOptionalText(
                request.Department);

        user.UpdatedAt =
            DateTime.UtcNow;

        /*
         * Kullanıcı tracking açık olarak getirildi.
         * Repository Update çağrısı işlemi açık biçimde ifade eder.
         */
        _userRepository.Update(user);

        await _userRepository.SaveChangesAsync(
            cancellationToken);

        return user.ToResponseDto();
    }

    public async Task<UserResponseDto> UpdateStatusAsync(
        int id,
        UpdateUserStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user =
            await _userRepository.GetByIdForUpdateAsync(
                id,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "Kullanıcı bulunamadı.");
        }

        if (user.IsActive == request.IsActive)
        {
            return user.ToResponseDto();
        }

        user.IsActive =
            request.IsActive;

        /*
         * Kullanıcı pasifleştirildiğinde veya yeniden aktifleştirildiğinde
         * daha önce oluşturulmuş access token'ların kullanımını engeller.
         */
        user.TokenVersion++;

        user.UpdatedAt =
            DateTime.UtcNow;

        _userRepository.Update(user);

        await _userRepository.SaveChangesAsync(
            cancellationToken);

        return user.ToResponseDto();
    }


    public async Task ResetPasswordAsync(
        int userId,
        ResetUserPasswordRequestDto request,
        string? revokedByIp,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user =
            await _userRepository.GetByIdForUpdateAsync(
                userId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "Şifresi sıfırlanacak kullanıcı bulunamadı.");
        }

        if (!user.IsActive)
        {
            throw new BusinessRuleException(
                "Pasif bir kullanıcının şifresi sıfırlanamaz.");
        }

    
        user.PasswordHash =
            _passwordHasher.Hash(
                request.NewPassword);

        user.TokenVersion++;

        user.UpdatedAt =
            DateTime.UtcNow;

        await _refreshTokenRepository.RevokeAllActiveTokensAsync(
            user.Id,
            DateTime.UtcNow,
            revokedByIp,
            "Kullanıcı şifresi yönetici tarafından sıfırlandı.",
            cancellationToken);

        _userRepository.Update(user);

 
        await _userRepository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task DeleteAsync(
        int userId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
     
        if (userId == currentUserId)
        {
            throw new BusinessRuleException(
                "Giriş yaptığınız kullanıcı hesabını silemezsiniz.");
        }

        var user =
            await _userRepository.GetByIdForUpdateAsync(
                userId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "Silinecek kullanıcı bulunamadı.");
        }

        _userRepository.Remove(user);

        await _userRepository.SaveChangesAsync(
            cancellationToken);
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
}