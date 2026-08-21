namespace ProjectManagement.Application.DTOs.Mailbox;

/// <summary>
/// Mailbox ekranlarında görüntülenecek temel kullanıcı
/// bilgilerini temsil eder.
/// </summary>
public sealed class MailboxUserDto
{
    public int Id { get; init; }

    public string FirstName { get; init; } =
        string.Empty;

    public string LastName { get; init; } =
        string.Empty;

    public string FullName { get; init; } =
        string.Empty;

    public string Email { get; init; } =
        string.Empty;
}