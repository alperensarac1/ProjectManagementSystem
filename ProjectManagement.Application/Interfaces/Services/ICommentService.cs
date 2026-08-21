using ProjectManagement.Application.DTOs.Comments;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Interfaces.Services;


public interface ICommentService
{
    Task<IReadOnlyCollection<CommentResponseDto>>
        GetByTaskIdAsync(
            int taskId,
            int currentUserId,
            UserRole currentUserRole,
            CancellationToken cancellationToken = default);

    Task<CommentResponseDto> CreateAsync(
        int taskId,
        CreateCommentRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<CommentResponseDto> UpdateAsync(
        int taskId,
        int commentId,
        UpdateCommentRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int taskId,
        int commentId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);
}