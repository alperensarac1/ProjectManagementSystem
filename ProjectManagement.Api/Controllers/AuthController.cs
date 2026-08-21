using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Extensions;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Auth;
using ProjectManagement.Application.DTOs.Users;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Mappings;

namespace ProjectManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    private readonly IValidator<RegisterRequestDto>
        _registerValidator;

    private readonly IValidator<LoginRequestDto>
        _loginValidator;

    private readonly IValidator<ChangePasswordRequestDto>
        _changePasswordValidator;

    private readonly IValidator<RefreshTokenRequestDto>
        _refreshTokenValidator;

    private readonly IValidator<LogoutRequestDto>
        _logoutValidator;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<ChangePasswordRequestDto> changePasswordValidator,
        IValidator<RefreshTokenRequestDto> refreshTokenValidator,
        IValidator<LogoutRequestDto> logoutValidator)
    {
        _authService = authService;
        _userRepository = userRepository;
        _currentUserService = currentUserService;

        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _logoutValidator = logoutValidator;
    }

    [AllowAnonymous]
    [EnableRateLimiting("authentication")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        await _registerValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var result =
            await _authService.RegisterAsync(
                request,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<AuthResponseDto>.Succeed(
                result,
                "Kullanıcı kaydı başarıyla oluşturuldu."));
    }

    [AllowAnonymous]
    [EnableRateLimiting("authentication")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        await _loginValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var result =
            await _authService.LoginAsync(
                request,
                cancellationToken);

        return Ok(
            ApiResponse<AuthResponseDto>.Succeed(
                result,
                "Giriş işlemi başarılı."));
    }


    [AllowAnonymous]
    [EnableRateLimiting("authentication")]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        await _refreshTokenValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var result =
            await _authService.RefreshTokenAsync(
                request,
                GetClientIpAddress(),
                cancellationToken);

        return Ok(
            ApiResponse<AuthResponseDto>.Succeed(
                result,
                "Token başarıyla yenilendi."));
    }

    [Authorize]
    [EnableRateLimiting("general")]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var user =
            await _userRepository.GetByIdAsync(
                currentUser.UserId,
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

        return Ok(
            ApiResponse<UserResponseDto>.Succeed(
                user.ToResponseDto(),
                "Kullanıcı bilgileri başarıyla getirildi."));
    }

    [Authorize]
    [EnableRateLimiting("general")]
    [HttpPatch("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        await _changePasswordValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        await _authService.ChangePasswordAsync(
            currentUser.UserId,
            request,
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                new { },
                "Şifreniz başarıyla değiştirildi. " +
                "Lütfen yeniden giriş yapınız."));
    }

    [Authorize]
    [EnableRateLimiting("general")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequestDto request,
        CancellationToken cancellationToken)
    {
        await _logoutValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        await _authService.LogoutAsync(
            currentUser.UserId,
            request,
            GetClientIpAddress(),
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                new { },
                "Çıkış işlemi başarıyla tamamlandı."));
    }


    [Authorize]
    [EnableRateLimiting("general")]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll(
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        await _authService.LogoutAllAsync(
            currentUser.UserId,
            GetClientIpAddress(),
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                new { },
                "Bütün cihazlardaki oturumlar kapatıldı."));
    }

    private string? GetClientIpAddress()
    {
        return HttpContext.Connection
            .RemoteIpAddress?
            .ToString();
    }
}