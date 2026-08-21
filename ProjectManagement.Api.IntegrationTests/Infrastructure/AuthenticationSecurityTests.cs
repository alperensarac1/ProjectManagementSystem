using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;


[Collection(ApiTestCollection.Name)]
public sealed class AuthenticationSecurityTests
{
    private readonly ProjectManagementApiFactory _factory;

    public AuthenticationSecurityTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ChangePassword_InvalidatesOldAccessToken()
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

        const string newPassword =
            "NewSecurePassword123";

        var changePasswordRequest = new
        {
            currentPassword =
                registerRequest.Password,

            newPassword,

            confirmNewPassword =
                newPassword
        };

        var changeResponse =
            await client.PatchAsJsonAsync(
                "/api/auth/change-password",
                changePasswordRequest);

        changeResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);


        var meResponse =
            await client.GetAsync(
                "/api/auth/me");

        meResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_InvalidatesOldRefreshToken()
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

        const string newPassword =
            "NewSecurePassword123";

        var changePasswordRequest = new
        {
            currentPassword =
                registerRequest.Password,

            newPassword,

            confirmNewPassword =
                newPassword
        };

        var changeResponse =
            await client.PatchAsJsonAsync(
                "/api/auth/change-password",
                changePasswordRequest);

        changeResponse.StatusCode
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
    public async Task ChangePassword_AllowsLoginWithNewPassword()
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

        const string newPassword =
            "NewSecurePassword123";

        var changePasswordRequest = new
        {
            currentPassword =
                registerRequest.Password,

            newPassword,

            confirmNewPassword =
                newPassword
        };

        var changeResponse =
            await client.PatchAsJsonAsync(
                "/api/auth/change-password",
                changePasswordRequest);

        changeResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        AuthenticationTestHelper.ClearBearerToken(
            client);

        var newAuthentication =
            await AuthenticationTestHelper.LoginAsync(
                client,
                registerRequest.Email,
                newPassword);

        newAuthentication.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();

        newAuthentication.RefreshToken
            .Should()
            .NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RefreshToken_CanOnlyBeUsedOnce()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authentication =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        var refreshRequest = new
        {
            refreshToken =
                authentication.RefreshToken
        };

        var firstRefreshResponse =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                refreshRequest);

        firstRefreshResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var secondRefreshResponse =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                refreshRequest);

        secondRefreshResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshTokenRotation_ReturnsDifferentRefreshToken()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var originalAuthentication =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        var refreshedAuthentication =
            await AuthenticationTestHelper.RefreshAsync(
                client,
                originalAuthentication.RefreshToken);

        refreshedAuthentication.RefreshToken
            .Should()
            .NotBeNullOrWhiteSpace();

        refreshedAuthentication.RefreshToken
            .Should()
            .NotBe(
                originalAuthentication.RefreshToken);
    }

    [Fact]
    public async Task LogoutAll_InvalidatesAccessAndRefreshTokens()
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
}