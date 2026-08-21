using System.Net;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;

[Collection(ApiTestCollection.Name)]
public sealed class AdminUserAuthorizationTests
{
    private readonly ProjectManagementApiFactory _factory;

    public AdminUserAuthorizationTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsSuccess()
    {

        using var client =
            _factory.CreateClient();

        var adminAuthentication =
            await AuthenticationTestHelper.LoginAsAdminAsync(
                client,
                _factory.Services);

        AuthenticationTestHelper.SetBearerToken(
            client,
            adminAuthentication.AccessToken);

        var response =
            await client.GetAsync(
                "/api/users");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }


    [Fact]
    public async Task GetUsers_AsTeamMember_ReturnsForbidden()
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

        var response =
            await client.GetAsync(
                "/api/users");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_WithoutAccessToken_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();

        var response =
            await client.GetAsync(
                "/api/users");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}