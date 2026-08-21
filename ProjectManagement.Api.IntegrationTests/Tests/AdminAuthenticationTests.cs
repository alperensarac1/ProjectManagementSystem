using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;

[Collection(ApiTestCollection.Name)]
public sealed class AdminAuthenticationTests
{
    private readonly ProjectManagementApiFactory _factory;

    public AdminAuthenticationTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_WithValidCredentials_CanLogin()
    {
        using var client =
            _factory.CreateClient();

        var authentication =
            await AuthenticationTestHelper
                .LoginAsAdminAsync(
                    client,
                    _factory.Services);

        authentication.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();

        authentication.RefreshToken
            .Should()
            .NotBeNullOrWhiteSpace();

        authentication.User
            .Should()
            .NotBeNull();

        authentication.User!.Email
            .Should()
            .Be(
                "integration.admin@projectmanagement.test");
    }

    [Fact]
    public async Task Admin_WithValidToken_CanReadCurrentUser()
    {
        using var client =
            _factory.CreateClient();

        var authentication =
            await AuthenticationTestHelper
                .LoginAsAdminAsync(
                    client,
                    _factory.Services);

        AuthenticationTestHelper.SetBearerToken(
            client,
            authentication.AccessToken);

        var response =
            await client.GetAsync(
                "/api/auth/me");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Admin_WithInvalidPassword_CannotLogin()
    {
        using var client =
            _factory.CreateClient();

        var admin =
            await TestAdminSeeder.SeedAsync(
                _factory.Services);

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    email = admin.Email,
                    password = "WrongAdminPassword123"
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}