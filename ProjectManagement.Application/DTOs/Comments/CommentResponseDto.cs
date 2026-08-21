namespace ProjectManagement.Application.DTOs.Comments;



public sealed class CommentResponseDto
{
    public int Id { get; init; }
    public int TaskId { get; init; }
    public int UserId { get; init; }
    public string UserFullName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }
}