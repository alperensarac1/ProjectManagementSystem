namespace ProjectManagement.Api.IntegrationTests.Models;


public sealed class AddProjectMemberTestRequest
{
    public int UserId { get; init; }

    public string Role { get; init; } =
        "Member";
}