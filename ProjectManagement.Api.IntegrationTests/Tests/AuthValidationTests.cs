using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;


[Collection(ApiTestCollection.Name)]
public sealed class AuthValidationTests
{
    private readonly HttpClient _client;

    public AuthValidationTests(
        ProjectManagementApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithInvalidRequest_ReturnsBadRequest()
    {
   
        var request = new
        {
            email = "gecersiz-email",
            password = "123"
        };

 
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

 
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        var body =
            await response.Content
                .ReadFromJsonAsync<ErrorResponse>();

        body.Should().NotBeNull();

        body!.Success.Should().BeFalse();

        body.Errors.Should().NotBeNull();

        body.Errors!
            .Should()
            .ContainKey("Email");

        body.Errors
            .Should()
            .ContainKey("Password");
    }

    private sealed class ErrorResponse
    {
        public bool Success { get; init; }

        public string Message { get; init; } =
            string.Empty;

        public object? Data { get; init; }

        public Dictionary<string, string[]>? Errors
        {
            get;
            init;
        }
    }
}