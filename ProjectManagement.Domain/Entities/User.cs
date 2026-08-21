using ProjectManagement.Domain.Common;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } =
        string.Empty;

    public string LastName { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;

    public string PasswordHash { get; set; } =
        string.Empty;

    public UserRole Role { get; set; }

    public string? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public int TokenVersion { get; set; }

    public ICollection<Project> OwnedProjects { get; set; } =
        new List<Project>();

    public ICollection<ProjectTask> CreatedTasks { get; set; } =
        new List<ProjectTask>();

    public ICollection<ProjectTask> AssignedTasks { get; set; } =
        new List<ProjectTask>();

    public ICollection<ProjectMember> ProjectMemberships { get; set; } =
        new List<ProjectMember>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } =
        new List<RefreshToken>();

    public ICollection<Comment> Comments { get; set; } =
        new List<Comment>();

    public ICollection<TaskHistory> TaskHistories { get; set; } =
        new List<TaskHistory>();

    public ICollection<TaskTimeLog> TaskTimeLogs { get; set; } =
        new List<TaskTimeLog>();

    /*
     * Kullanıcının gönderdiği uygulama içi mesajlar.
     */
    public ICollection<MailboxMessage> SentMailboxMessages { get; set; } =
        new List<MailboxMessage>();

    /*
     * Kullanıcının alıcısı olduğu mesaj kayıtları.
     *
     * Bir mesajın birden fazla alıcısı olabileceği için doğrudan
     * MailboxMessage koleksiyonu yerine ara entity kullanıyoruz.
     */
    public ICollection<MailboxRecipient> MailboxRecipients { get; set; } =
        new List<MailboxRecipient>();
}