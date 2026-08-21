namespace ProjectManagement.Api.IntegrationTests.Models;


public sealed class CommentResponseModel
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public string UserFullName { get; set; } =
        string.Empty;

    public string UserEmail { get; set; } =
        string.Empty;

    public string Content { get; set; } =
        string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }
}