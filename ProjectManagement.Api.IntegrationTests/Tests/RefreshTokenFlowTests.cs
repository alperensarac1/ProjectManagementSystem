using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;

[Collection(ApiTestCollection.Name)]
public sealed class RefreshTokenFlowTests
{
    private readonly ProjectManagementApiFactory _factory;

    public RefreshTokenFlowTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }


    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokenPair()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var originalAuth =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        var refreshRequest = new
        {
            refreshToken =
                originalAuth.RefreshToken
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                refreshRequest);

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
            .NotBe(originalAuth.RefreshToken);
    }


    [Fact]
    public async Task Refresh_WithPreviouslyUsedToken_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var originalAuth =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        var refreshRequest = new
        {
            refreshToken =
                originalAuth.RefreshToken
        };

        var firstResponse =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                refreshRequest);

        firstResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);


        var secondResponse =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                refreshRequest);


        secondResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();

        var request = new
        {
            refreshToken =
                "invalid-refresh-token-value"
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
    public async Task Refresh_WithEmptyToken_ReturnsBadRequest()
    {
        using var client =
            _factory.CreateClient();

        var request = new
        {
            refreshToken = string.Empty
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
        body!.Success.Should().BeFalse();

        body.Errors
            .Should()
            .NotBeNull();

        body.Errors!
            .Should()
            .ContainKey("RefreshToken");
    }

    [Fact]
    public async Task Logout_WithValidRefreshToken_RevokesToken()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authResult =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                authResult.AccessToken);

        var logoutRequest = new
        {
            refreshToken =
                authResult.RefreshToken
        };

        var logoutResponse =
            await client.PostAsJsonAsync(
                "/api/auth/logout",
                logoutRequest);

        logoutResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        client.DefaultRequestHeaders.Authorization = null;

        var refreshResponse =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                new
                {
                    refreshToken =
                        authResult.RefreshToken
                });

        refreshResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithoutAccessToken_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();

        var request = new
        {
            refreshToken =
                "some-refresh-token"
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/logout",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LogoutAll_RevokesAccessAndRefreshTokens()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authResult =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                authResult.AccessToken);

        var logoutAllResponse =
            await client.PostAsync(
                "/api/auth/logout-all",
                content: null);

        logoutAllResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);


        var meResponse =
            await client.GetAsync(
                "/api/auth/me");

        meResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);

        client.DefaultRequestHeaders.Authorization = null;

        var refreshResponse =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                new
                {
                    refreshToken =
                        authResult.RefreshToken
                });

        refreshResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}