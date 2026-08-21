using ProjectManagement.Domain.Common;

namespace ProjectManagement.Domain.Entities;

public class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty;

    public int TaskId { get; set; }
    public int UserId { get; set; }
    public ProjectTask Task { get; set; } = null!;
    public User User { get; set; } = null!;
}