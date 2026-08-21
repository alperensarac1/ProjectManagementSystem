using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class ApiTestCollection
    : ICollectionFixture<ProjectManagementApiFactory>
{
    public const string Name =
        "Project Management API Integration Tests";
}