using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjectManagement.Application.Common.Extensions;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Users;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("general")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    private readonly IValidator<CreateUserRequestDto>
        _createValidator;

    private readonly IValidator<UpdateUserRequestDto>
        _updateValidator;

    private readonly IValidator<UserListQueryDto>
        _listQueryValidator;

    private readonly IValidator<ResetUserPasswordRequestDto>
        _resetPasswordValidator;

    public UsersController(
        IUserService userService,
        ICurrentUserService currentUserService,
        IValidator<CreateUserRequestDto> createValidator,
        IValidator<UpdateUserRequestDto> updateValidator,
        IValidator<UserListQueryDto> listQueryValidator,
        IValidator<ResetUserPasswordRequestDto>
            resetPasswordValidator)
    {
        _userService = userService;
        _currentUserService = currentUserService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _listQueryValidator = listQueryValidator;
        _resetPasswordValidator = resetPasswordValidator;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<UserResponseDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] UserListQueryDto query,
        CancellationToken cancellationToken)
    {
        await _listQueryValidator.ValidateAndThrowAppAsync(
            query,
            cancellationToken);

        var result =
            await _userService.GetPagedAsync(
                query,
                cancellationToken);

        return Ok(
            ApiResponse<PagedResult<UserResponseDto>>.Succeed(
                result,
                "Kullanıcılar başarıyla getirildi."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<UserResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result =
            await _userService.GetByIdAsync(
                id,
                cancellationToken);

        return Ok(
            ApiResponse<UserResponseDto>.Succeed(
                result,
                "Kullanıcı bilgileri başarıyla getirildi."));
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<UserResponseDto>),
        StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var result =
            await _userService.CreateAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<UserResponseDto>.Succeed(
                result,
                "Kullanıcı başarıyla oluşturuldu."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<UserResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var result =
            await _userService.UpdateAsync(
                id,
                request,
                cancellationToken);

        return Ok(
            ApiResponse<UserResponseDto>.Succeed(
                result,
                "Kullanıcı başarıyla güncellendi."));
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(
        typeof(ApiResponse<UserResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateUserStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var result =
            await _userService.UpdateStatusAsync(
                id,
                request,
                cancellationToken);

        var message =
            request.IsActive
                ? "Kullanıcı hesabı aktif hâle getirildi."
                : "Kullanıcı hesabı pasif hâle getirildi.";

        return Ok(
            ApiResponse<UserResponseDto>.Succeed(
                result,
                message));
    }
    
    [HttpPatch("{id:int}/reset-password")]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(
        int id,
        [FromBody] ResetUserPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        await _resetPasswordValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);
        
        var revokedByIp =
            HttpContext.Connection
                .RemoteIpAddress?
                .ToString();

        await _userService.ResetPasswordAsync(
            id,
            request,
            revokedByIp,
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                new { },
                "Kullanıcının şifresi başarıyla sıfırlandı."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        await _userService.DeleteAsync(
            id,
            currentUser.UserId,
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                new { },
                "Kullanıcı başarıyla silindi."));
    }
}