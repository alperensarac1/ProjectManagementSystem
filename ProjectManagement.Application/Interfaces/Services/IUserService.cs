using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Users;

namespace ProjectManagement.Application.Interfaces.Services;


public interface IUserService
{
    Task<PagedResult<UserResponseDto>> GetPagedAsync(
        UserListQueryDto query,
        CancellationToken cancellationToken = default);

    Task<UserResponseDto> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<UserResponseDto> CreateAsync(
        CreateUserRequestDto request,
        CancellationToken cancellationToken = default);

    Task<UserResponseDto> UpdateAsync(
        int userId,
        UpdateUserRequestDto request,
        CancellationToken cancellationToken = default);

    Task<UserResponseDto> UpdateStatusAsync(
        int userId,
        UpdateUserStatusRequestDto request,
        CancellationToken cancellationToken = default);
    
    Task ResetPasswordAsync(
        int userId,
        ResetUserPasswordRequestDto request,
        string? revokedByIp,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int userId,
        int currentUserId,
        CancellationToken cancellationToken = default);
}