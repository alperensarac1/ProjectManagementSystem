using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;

[Collection(ApiTestCollection.Name)]
public sealed class RefreshTokenTests
{
    private readonly ProjectManagementApiFactory _factory;

    public RefreshTokenTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_WithValidRequest_ReturnsRefreshToken()
    {
        using var client =
            _factory.CreateClient();

        var request =
            TestUserFactory.CreateRegisterRequest();

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<AuthResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();

        body.Data.RefreshToken
            .Should()
            .NotBeNullOrWhiteSpace();

        body.Data.RefreshToken
            .Should()
            .NotBe(body.Data.AccessToken);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokenPair()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authentication =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        var request = new
        {
            refreshToken =
                authentication.RefreshToken
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<AuthResponseModel>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        body.Data!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();

        body.Data.RefreshToken
            .Should()
            .NotBeNullOrWhiteSpace();

        body.Data.RefreshToken
            .Should()
            .NotBe(authentication.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WhenOldRotatedTokenIsReused_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authentication =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        var request = new
        {
            refreshToken =
                authentication.RefreshToken
        };

        var firstRefreshResponse =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                request);

        firstRefreshResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var secondRefreshResponse =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                request);

        secondRefreshResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithUnknownToken_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();

        var request = new
        {
            refreshToken =
                $"unknown-{Guid.NewGuid():N}"
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithoutTokenValue_ReturnsBadRequest()
    {
        using var client =
            _factory.CreateClient();

        var request = new
        {
            refreshToken = ""
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<object>>();

        body.Should().NotBeNull();
        body!.Errors.Should().NotBeNull();

        body.Errors!
            .Should()
            .ContainKey("RefreshToken");
    }
}