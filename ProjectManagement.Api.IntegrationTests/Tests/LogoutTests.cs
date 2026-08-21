using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;

[Collection(ApiTestCollection.Name)]
public sealed class LogoutTests
{
    private readonly ProjectManagementApiFactory _factory;

    public LogoutTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Logout_WithValidRefreshToken_ReturnsSuccess()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authentication =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        AuthenticationTestHelper.SetBearerToken(
            client,
            authentication.AccessToken);

        var request = new
        {
            refreshToken =
                authentication.RefreshToken
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/logout",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_AfterLogout_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authentication =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        AuthenticationTestHelper.SetBearerToken(
            client,
            authentication.AccessToken);

        var logoutRequest = new
        {
            refreshToken =
                authentication.RefreshToken
        };

        var logoutResponse =
            await client.PostAsJsonAsync(
                "/api/auth/logout",
                logoutRequest);

        logoutResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        AuthenticationTestHelper.ClearBearerToken(
            client);

        var refreshRequest = new
        {
            refreshToken =
                authentication.RefreshToken
        };

        var refreshResponse =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                refreshRequest);

        refreshResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LogoutAll_InvalidatesCurrentAccessToken()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authentication =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        AuthenticationTestHelper.SetBearerToken(
            client,
            authentication.AccessToken);

        var logoutResponse =
            await client.PostAsync(
                "/api/auth/logout-all",
                content: null);

        logoutResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        /*
         * Aynı access token artık TokenVersion nedeniyle
         * geçersiz olmalıdır.
         */
        var meResponse =
            await client.GetAsync(
                "/api/auth/me");

        meResponse.StatusCode
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
                $"test-{Guid.NewGuid():N}"
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
    public async Task LogoutAll_WithoutAccessToken_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();

        var response =
            await client.PostAsync(
                "/api/auth/logout-all",
                content: null);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}