namespace ProjectManagement.Api.IntegrationTests.Models;

public sealed class CreateUserTestRequest
{
    public string FirstName { get; init; } =
        string.Empty;

    public string LastName { get; init; } =
        string.Empty;

    public string Email { get; init; } =
        string.Empty;

    public string Password { get; init; } =
        string.Empty;

    public string Role { get; init; } =
        string.Empty;

    public string? Department { get; init; }

    public bool IsActive { get; init; } = true;
}