using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;


[Collection(ApiTestCollection.Name)]
public sealed class AdminUserManagementTests
{
    private readonly ProjectManagementApiFactory _factory;

    public AdminUserManagementTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateUser_AsAdmin_WithProjectManagerRole_ReturnsCreated()
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

        var request =
            TestAdminUserFactory.CreateProjectManagerRequest();


        var response =
            await client.PostAsJsonAsync(
                "/api/users",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

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
            .Be(request.FirstName);

        body.Data.LastName
            .Should()
            .Be(request.LastName);

        body.Data.Email
            .Should()
            .Be(request.Email.ToLowerInvariant());

        body.Data.Role
            .Should()
            .Be("ProjectManager");

        body.Data.IsActive
            .Should()
            .BeTrue();
    }


    [Fact]
    public async Task CreateUser_AsAdmin_WithTeamMemberRole_ReturnsCreated()
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

        var request =
            TestAdminUserFactory.CreateTeamMemberRequest();

        // Act
        var response =
            await client.PostAsJsonAsync(
                "/api/users",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<UserResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.Role
            .Should()
            .Be("TeamMember");

        body.Data.Department
            .Should()
            .Be(request.Department);
    }

    [Fact]
    public async Task CreateUser_AsTeamMember_ReturnsForbidden()
    {

        using var client =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var teamMemberAuthentication =
            await AuthenticationTestHelper.RegisterAsync(
                client,
                registerRequest);

        AuthenticationTestHelper.SetBearerToken(
            client,
            teamMemberAuthentication.AccessToken);

        var createRequest =
            TestAdminUserFactory.CreateProjectManagerRequest();

        var response =
            await client.PostAsJsonAsync(
                "/api/users",
                createRequest);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUser_WithExistingEmail_ReturnsConflict()
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

        var request =
            TestAdminUserFactory.CreateTeamMemberRequest();

        var firstResponse =
            await client.PostAsJsonAsync(
                "/api/users",
                request);

        firstResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);


        var secondResponse =
            await client.PostAsJsonAsync(
                "/api/users",
                request);


        secondResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }


    [Fact]
    public async Task GetUserById_AsAdmin_ReturnsCreatedUser()
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

        var createRequest =
            TestAdminUserFactory.CreateTeamMemberRequest();

        var createResponse =
            await client.PostAsJsonAsync(
                "/api/users",
                createRequest);

        var createBody =
            await createResponse.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<UserResponseModel>>();

        createBody.Should().NotBeNull();
        createBody!.Data.Should().NotBeNull();

        var createdUserId =
            createBody.Data!.Id;


        var response =
            await client.GetAsync(
                $"/api/users/{createdUserId}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<UserResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.Id
            .Should()
            .Be(createdUserId);

        body.Data.Email
            .Should()
            .Be(createRequest.Email.ToLowerInvariant());
    }


    [Fact]
    public async Task UpdateUser_AsAdmin_ChangesRoleAndProfile()
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

        var createRequest =
            TestAdminUserFactory.CreateTeamMemberRequest();

        var createResponse =
            await client.PostAsJsonAsync(
                "/api/users",
                createRequest);

        var createBody =
            await createResponse.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<UserResponseModel>>();

        createBody.Should().NotBeNull();
        createBody!.Data.Should().NotBeNull();

        var createdUserId =
            createBody.Data!.Id;

        var updateRequest = new
        {
            firstName = "Updated",
            lastName = "Manager",
            email = createRequest.Email,
            role = "ProjectManager",
            department = "Project Management"
        };

        var response =
            await client.PutAsJsonAsync(
                $"/api/users/{createdUserId}",
                updateRequest);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<UserResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.FirstName
            .Should()
            .Be("Updated");

        body.Data.LastName
            .Should()
            .Be("Manager");

        body.Data.Role
            .Should()
            .Be("ProjectManager");

        body.Data.Department
            .Should()
            .Be("Project Management");
    }


    [Fact]
    public async Task UpdateStatus_AsAdmin_DeactivatesUser()
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

        var createRequest =
            TestAdminUserFactory.CreateTeamMemberRequest();

        var createResponse =
            await client.PostAsJsonAsync(
                "/api/users",
                createRequest);

        var createBody =
            await createResponse.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<UserResponseModel>>();

        createBody.Should().NotBeNull();
        createBody!.Data.Should().NotBeNull();

        var createdUserId =
            createBody.Data!.Id;

        var statusRequest = new
        {
            isActive = false
        };

        // Act
        var response =
            await client.PatchAsJsonAsync(
                $"/api/users/{createdUserId}/status",
                statusRequest);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<UserResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.IsActive
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task DeactivatedUser_WithExistingToken_ReturnsUnauthorized()
    {

        using var userClient =
            _factory.CreateClient();

        var registerRequest =
            TestUserFactory.CreateRegisterRequest();

        var userAuthentication =
            await AuthenticationTestHelper.RegisterAsync(
                userClient,
                registerRequest);

        var userId =
            userAuthentication.User!.Id;

        using var adminClient =
            _factory.CreateClient();

        var adminAuthentication =
            await AuthenticationTestHelper.LoginAsAdminAsync(
                adminClient,
                _factory.Services);

        AuthenticationTestHelper.SetBearerToken(
            adminClient,
            adminAuthentication.AccessToken);

        var statusRequest = new
        {
            isActive = false
        };

        var deactivateResponse =
            await adminClient.PatchAsJsonAsync(
                $"/api/users/{userId}/status",
                statusRequest);

        deactivateResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

    
        AuthenticationTestHelper.SetBearerToken(
            userClient,
            userAuthentication.AccessToken);


        var response =
            await userClient.GetAsync(
                "/api/auth/me");

    
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}