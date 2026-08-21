using System.Net;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;


[Collection(ApiTestCollection.Name)]
public sealed class AuthorizationTests
{
    private readonly HttpClient _client;

    public AuthorizationTests(
        ProjectManagementApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/projects")]
    [InlineData("/api/tasks")]
    [InlineData("/api/dashboard/summary")]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized(
        string endpoint)
    {
        // Act
        var response =
            await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}