namespace ProjectManagement.Api.IntegrationTests.Models;

public sealed class MailboxUserResponseModel
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