using ProjectManagement.Application.DTOs.Comments;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Mappings;
public static class CommentMappingExtensions
{
    public static CommentResponseDto ToResponseDto(
        this Comment comment,
        int currentUserId,
        UserRole currentUserRole,
        int projectOwnerId)
    {
        ArgumentNullException.ThrowIfNull(comment);

        var isCommentOwner =
            comment.UserId == currentUserId;

        var canDelete =
            isCommentOwner ||
            currentUserRole == UserRole.Admin ||
            projectOwnerId == currentUserId;

        return new CommentResponseDto
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            UserId = comment.UserId,

            UserFullName = comment.User is null
                ? string.Empty
                : $"{comment.User.FirstName} {comment.User.LastName}".Trim(),

            UserEmail = comment.User?.Email ?? string.Empty,

            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,

            /*
             * Yorum içeriğini yalnızca yorumun sahibi değiştirebilir.
             */
            CanEdit = isCommentOwner,

            /*
             * Admin, proje sahibi veya yorum sahibi yorumu silebilir.
             */
            CanDelete = canDelete
        };
    }
}