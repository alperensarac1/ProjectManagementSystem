using ProjectManagement.Api.IntegrationTests.Models;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;

public static class TestAdminUserFactory
{
    public static CreateUserTestRequest
        CreateProjectManagerRequest()
    {
        var uniqueValue =
            Guid.NewGuid().ToString("N");

        return new CreateUserTestRequest
        {
            FirstName = "Integration",
            LastName = "ProjectManager",

            Email =
                $"project-manager-{uniqueValue}@test.local",

            Password =
                "ProjectManagerPassword123",

            Role = "ProjectManager",
            Department = "Software",
            IsActive = true
        };
    }

    public static CreateUserTestRequest
        CreateTeamMemberRequest()
    {
        var uniqueValue =
            Guid.NewGuid().ToString("N");

        return new CreateUserTestRequest
        {
            FirstName = "Integration",
            LastName = "TeamMember",

            Email =
                $"team-member-{uniqueValue}@test.local",

            Password =
                "TeamMemberPassword123",

            Role = "TeamMember",
            Department = "Development",
            IsActive = true
        };
    }
}