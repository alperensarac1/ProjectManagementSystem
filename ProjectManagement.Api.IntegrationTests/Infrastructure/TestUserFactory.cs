namespace ProjectManagement.Api.IntegrationTests.Infrastructure;


public static class TestUserFactory
{
    public static RegisterTestRequest CreateRegisterRequest()
    {
        var uniquePart =
            Guid.NewGuid()
                .ToString("N");

        return new RegisterTestRequest
        {
            FirstName = "Integration",
            LastName = "User",

            Email =
                $"integration-{uniquePart}@test.local",

            Password = "Integration12345",

            Department = "Test Department"
        };
    }
}

public sealed class RegisterTestRequest
{
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string? Department { get; init; }
}

public sealed class LoginTestRequest
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}