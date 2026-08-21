using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;

[Collection(ApiTestCollection.Name)]
public sealed class ChangePasswordTests
{
    private readonly ProjectManagementApiFactory _factory;

    public ChangePasswordTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ChangePassword_WithValidCurrentPassword_ReturnsSuccess()
    {
 
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authResult =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        AuthenticationTestHelper.SetBearerToken(
            client,
            authResult.AccessToken);

        var request = new
        {
            currentPassword =
                registerRequest.Password,

            newPassword =
                "NewIntegrationPassword123",

            confirmNewPassword =
                "NewIntegrationPassword123"
        };

        var response =
            await client.PatchAsJsonAsync(
                "/api/auth/change-password",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<object>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Login_AfterPasswordChange_OldPasswordIsRejected()
    {
        // Arrange
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authResult =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        AuthenticationTestHelper.SetBearerToken(
            client,
            authResult.AccessToken);

        const string newPassword =
            "ChangedPassword123";

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

        var oldPasswordLoginRequest = new
        {
            email = registerRequest.Email,
            password = registerRequest.Password
        };

        // Act
        var response =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                oldPasswordLoginRequest);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_AfterPasswordChange_NewPasswordIsAccepted()
    {
        // Arrange
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authResult =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        AuthenticationTestHelper.SetBearerToken(
            client,
            authResult.AccessToken);

        const string newPassword =
            "ChangedPassword123";

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

        var newPasswordLoginRequest = new
        {
            email = registerRequest.Email,
            password = newPassword
        };

        // Act
        var response =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                newPasswordLoginRequest);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ReturnsUnauthorized()
    {
        // Arrange
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authResult =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        AuthenticationTestHelper.SetBearerToken(
            client,
            authResult.AccessToken);

        var request = new
        {
            currentPassword =
                "WrongCurrentPassword123",

            newPassword =
                "NewIntegrationPassword123",

            confirmNewPassword =
                "NewIntegrationPassword123"
        };

        // Act
        var response =
            await client.PatchAsJsonAsync(
                "/api/auth/change-password",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithMismatchedConfirmation_ReturnsBadRequest()
    {
        // Arrange
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var authResult =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        AuthenticationTestHelper.SetBearerToken(
            client,
            authResult.AccessToken);

        var request = new
        {
            currentPassword =
                registerRequest.Password,

            newPassword =
                "NewIntegrationPassword123",

            confirmNewPassword =
                "DifferentPassword123"
        };

        // Act
        var response =
            await client.PatchAsJsonAsync(
                "/api/auth/change-password",
                request);

        // Assert
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
            .ContainKey("ConfirmNewPassword");
    }

    [Fact]
    public async Task ChangePassword_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        using var client =
            _factory.CreateClient();

        var request = new
        {
            currentPassword =
                "CurrentPassword123",

            newPassword =
                "NewPassword123",

            confirmNewPassword =
                "NewPassword123"
        };

        // Act
        var response =
            await client.PatchAsJsonAsync(
                "/api/auth/change-password",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}