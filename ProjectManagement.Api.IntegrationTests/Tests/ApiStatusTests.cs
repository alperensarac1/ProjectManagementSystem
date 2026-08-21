using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;


[Collection(ApiTestCollection.Name)]
public sealed class ApiStatusTests
{
    private readonly HttpClient _client;

    public ApiStatusTests(
        ProjectManagementApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RootEndpoint_WhenApiIsRunning_ReturnsApiInformation()
    {
    
        var response =
            await _client.GetAsync("/");


        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<ApiStatusResponse>();

        body.Should().NotBeNull();

        body!.Message
            .Should()
            .Be("Project Management API çalışıyor.");

        body.Database
            .Should()
            .Be("SQLite");

        body.Authentication
            .Should()
            .Be("JWT Bearer");

        body.ActiveUserValidation
            .Should()
            .BeTrue();

        body.Cors
            .Should()
            .BeTrue();

        body.RateLimiting
            .Should()
            .BeTrue();

        body.HealthChecks
            .Should()
            .BeTrue();
    }

    private sealed class ApiStatusResponse
    {
        public string Message { get; init; } =
            string.Empty;

        public string Database { get; init; } =
            string.Empty;

        public string Authentication { get; init; } =
            string.Empty;

        public bool ActiveUserValidation { get; init; }

        public bool Cors { get; init; }

        public bool RateLimiting { get; init; }

        public bool HealthChecks { get; init; }

        public string Environment { get; init; } =
            string.Empty;

        public DateTime UtcTime { get; init; }
    }
}