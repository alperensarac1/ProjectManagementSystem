using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;


[Collection(ApiTestCollection.Name)]
public sealed class HealthCheckTests
{
    private readonly HttpClient _client;

    public HealthCheckTests(
        ProjectManagementApiFactory factory)
    {

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LiveHealthCheck_WhenApplicationIsRunning_ReturnsHealthy()
    {

        var response =
            await _client.GetAsync(
                "/health/live");


        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<HealthResponse>();

        body.Should().NotBeNull();

        body!.Status.Should().Be("Healthy");

        body.Checks
            .Should()
            .Contain(check =>
                check.Name == "application" &&
                check.Status == "Healthy");
    }

    [Fact]
    public async Task ReadyHealthCheck_WhenDatabaseIsAvailable_ReturnsHealthy()
    {

        var response =
            await _client.GetAsync(
                "/health/ready");


        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<HealthResponse>();

        body.Should().NotBeNull();

        body!.Status.Should().Be("Healthy");

        body.Checks
            .Should()
            .Contain(check =>
                check.Name == "database" &&
                check.Status == "Healthy");
    }

    [Fact]
    public async Task FullHealthCheck_WhenApplicationAndDatabaseAreReady_ReturnsHealthy()
    {
 
        var response =
            await _client.GetAsync(
                "/health");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<HealthResponse>();

        body.Should().NotBeNull();

        body!.Status.Should().Be("Healthy");

        body.Checks.Should().HaveCount(2);
    }

    private sealed class HealthResponse
    {
        public string Status { get; init; } =
            string.Empty;

        public IReadOnlyCollection<HealthCheckItem> Checks
        {
            get;
            init;
        } = [];
    }

    private sealed class HealthCheckItem
    {
        public string Name { get; init; } =
            string.Empty;

        public string Status { get; init; } =
            string.Empty;

        public string? Description { get; init; }
    }
}