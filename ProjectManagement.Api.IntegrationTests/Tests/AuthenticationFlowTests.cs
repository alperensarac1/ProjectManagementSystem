using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;


[Collection(ApiTestCollection.Name)]
public sealed class AuthenticationFlowTests
{
    private readonly ProjectManagementApiFactory _factory;

    public AuthenticationFlowTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }


    [Fact]
    public async Task Register_WithValidRequest_ReturnsCreatedUserAndToken()
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
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        body.Data!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();

        body.Data.RefreshToken
            .Should()
            .NotBeNullOrWhiteSpace();

        body.Data.TokenType
            .Should()
            .Be("Bearer");

        body.Data.User
            .Should()
            .NotBeNull();

        body.Data.User!.Id
            .Should()
            .BeGreaterThan(0);

        body.Data.User.FirstName
            .Should()
            .Be(request.FirstName);

        body.Data.User.LastName
            .Should()
            .Be(request.LastName);

        body.Data.User.FullName
            .Should()
            .Be(
                $"{request.FirstName} " +
                $"{request.LastName}");

        body.Data.User.Email
            .Should()
            .Be(request.Email.ToLowerInvariant());

        body.Data.User.Role
            .Should()
            .Be("TeamMember");

        body.Data.User.IsActive
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task Login_WithRegisteredUser_ReturnsJwtToken()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        await AuthenticationTestHelper.RegisterAsync(
            client,
            registerRequest);

        AuthenticationTestHelper.ClearBearerToken(
            client);

        var loginRequest = new LoginTestRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

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

        body.Data.TokenType
            .Should()
            .Be("Bearer");

        body.Data.User
            .Should()
            .NotBeNull();

        body.Data.User!.Email
            .Should()
            .Be(registerRequest.Email.ToLowerInvariant());

        body.Data.User.Role
            .Should()
            .Be("TeamMember");

        body.Data.User.IsActive
            .Should()
            .BeTrue();
    }


    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsAuthenticatedUser()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        await AuthenticationTestHelper.RegisterAsync(
            client,
            registerRequest);

        var loginResult =
            await AuthenticationTestHelper.LoginAsync(
                client,
                registerRequest.Email,
                registerRequest.Password);

        AuthenticationTestHelper.SetBearerToken(
            client,
            loginResult.AccessToken);

        var response =
            await client.GetAsync(
                "/api/auth/me");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<UserResponseModel>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        body.Data!.Id
            .Should()
            .BeGreaterThan(0);

        body.Data.FirstName
            .Should()
            .Be(registerRequest.FirstName);

        body.Data.LastName
            .Should()
            .Be(registerRequest.LastName);

        body.Data.Email
            .Should()
            .Be(registerRequest.Email.ToLowerInvariant());

        body.Data.FullName
            .Should()
            .Be(
                $"{registerRequest.FirstName} " +
                $"{registerRequest.LastName}");

        body.Data.Role
            .Should()
            .Be("TeamMember");

        body.Data.IsActive
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsConflict()
    {
        using var client =
            _factory.CreateClient();

        var request =
            TestUserFactory.CreateRegisterRequest();

        var firstResponse =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        firstResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var secondResponse =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        secondResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);

        var body =
            await secondResponse.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<object>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeFalse();

        body.Message
            .Should()
            .NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        await AuthenticationTestHelper.RegisterAsync(
            client,
            registerRequest);

        AuthenticationTestHelper.ClearBearerToken(
            client);

        var loginRequest = new LoginTestRequest
        {
            Email = registerRequest.Email,
            Password = "WrongPassword123"
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<object>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeFalse();

        body.Message
            .Should()
            .NotBeNullOrWhiteSpace();
    }


    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();

        var loginRequest = new LoginTestRequest
        {
            Email =
                $"unknown-{Guid.NewGuid():N}@test.local",

            Password =
                "UnknownPassword123"
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<object>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeFalse();

        body.Message
            .Should()
            .NotBeNullOrWhiteSpace();
    }
}