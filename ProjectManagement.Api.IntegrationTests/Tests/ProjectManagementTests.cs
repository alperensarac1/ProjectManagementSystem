using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;


[Collection(ApiTestCollection.Name)]
public sealed class ProjectManagementTests
{
    private readonly ProjectManagementApiFactory _factory;

    public ProjectManagementTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateProject_AsProjectManager_ReturnsCreated()
    {
        using var adminClient =
            _factory.CreateClient();

        using var projectManagerClient =
            _factory.CreateClient();

        var projectManager =
            await ProjectManagerTestHelper.CreateAndLoginAsync(
                adminClient,
                projectManagerClient,
                _factory.Services);

        AuthenticationTestHelper.SetBearerToken(
            projectManagerClient,
            projectManager.Authentication.AccessToken);

        var request =
            TestProjectFactory.CreateValidRequest();

        var response =
            await projectManagerClient.PostAsJsonAsync(
                "/api/projects",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<ProjectResponseModel>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        body.Data!.Id
            .Should()
            .BeGreaterThan(0);

        body.Data.Name
            .Should()
            .Be(request.Name);

        body.Data.Description
            .Should()
            .Be(request.Description);

        body.Data.Status
            .Should()
            .Be("Planning");

        body.Data.OwnerId
            .Should()
            .Be(projectManager.User.Id);

        body.Data.OwnerEmail
            .Should()
            .Be(projectManager.User.Email);

        body.Data.IsArchived
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task CreateProject_AsAdmin_ReturnsCreated()
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
            TestProjectFactory.CreateValidRequest(
                adminAuthentication.User!.Id);

        var response =
            await client.PostAsJsonAsync(
                "/api/projects",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateProject_AsTeamMember_ReturnsForbidden()
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

        var request =
            TestProjectFactory.CreateValidRequest();

        var response =
            await client.PostAsJsonAsync(
                "/api/projects",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProjectById_AsOwner_ReturnsProject()
    {
        using var adminClient =
            _factory.CreateClient();

        using var projectManagerClient =
            _factory.CreateClient();

        var projectManager =
            await ProjectManagerTestHelper.CreateAndLoginAsync(
                adminClient,
                projectManagerClient,
                _factory.Services);

        AuthenticationTestHelper.SetBearerToken(
            projectManagerClient,
            projectManager.Authentication.AccessToken);

        var request =
            TestProjectFactory.CreateValidRequest();

        var createResponse =
            await projectManagerClient.PostAsJsonAsync(
                "/api/projects",
                request);

        var createBody =
            await createResponse.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<ProjectResponseModel>>();

        createBody.Should().NotBeNull();
        createBody!.Data.Should().NotBeNull();

        var projectId =
            createBody.Data!.Id;

        var response =
            await projectManagerClient.GetAsync(
                $"/api/projects/{projectId}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<ProjectResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.Id
            .Should()
            .Be(projectId);

        body.Data.Name
            .Should()
            .Be(request.Name);
    }

    [Fact]
    public async Task UpdateProject_AsOwner_ChangesProject()
    {
        using var adminClient =
            _factory.CreateClient();

        using var projectManagerClient =
            _factory.CreateClient();

        var projectManager =
            await ProjectManagerTestHelper.CreateAndLoginAsync(
                adminClient,
                projectManagerClient,
                _factory.Services);

        AuthenticationTestHelper.SetBearerToken(
            projectManagerClient,
            projectManager.Authentication.AccessToken);

        var createRequest =
            TestProjectFactory.CreateValidRequest();

        var createResponse =
            await projectManagerClient.PostAsJsonAsync(
                "/api/projects",
                createRequest);

        var createBody =
            await createResponse.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<ProjectResponseModel>>();

        createBody.Should().NotBeNull();
        createBody!.Data.Should().NotBeNull();

        var projectId =
            createBody.Data!.Id;

        var updateRequest =
            TestProjectFactory.CreateUpdateRequest(
                projectManager.User.Id);

        var response =
            await projectManagerClient.PutAsJsonAsync(
                $"/api/projects/{projectId}",
                updateRequest);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<ProjectResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.Name
            .Should()
            .Be(updateRequest.Name);

        body.Data.Description
            .Should()
            .Be(updateRequest.Description);

        body.Data.Status
            .Should()
            .Be("Active");

        body.Data.OwnerId
            .Should()
            .Be(projectManager.User.Id);
    }

    [Fact]
    public async Task ArchiveProject_AsOwner_ArchivesProject()
    {
        using var adminClient =
            _factory.CreateClient();

        using var projectManagerClient =
            _factory.CreateClient();

        var projectManager =
            await ProjectManagerTestHelper.CreateAndLoginAsync(
                adminClient,
                projectManagerClient,
                _factory.Services);

        AuthenticationTestHelper.SetBearerToken(
            projectManagerClient,
            projectManager.Authentication.AccessToken);

        var createResponse =
            await projectManagerClient.PostAsJsonAsync(
                "/api/projects",
                TestProjectFactory.CreateValidRequest());

        var createBody =
            await createResponse.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<ProjectResponseModel>>();

        createBody.Should().NotBeNull();
        createBody!.Data.Should().NotBeNull();

        var projectId =
            createBody.Data!.Id;

        var archiveRequest = new
        {
            isArchived = true
        };

        var response =
            await projectManagerClient.PatchAsJsonAsync(
                $"/api/projects/{projectId}/archive",
                archiveRequest);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<ProjectResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.IsArchived
            .Should()
            .BeTrue();

        body.Data.ArchivedAt
            .Should()
            .NotBeNull();
    }

    [Fact]
    public async Task CreateProject_WithInvalidDates_ReturnsBadRequest()
    {
        using var adminClient =
            _factory.CreateClient();

        using var projectManagerClient =
            _factory.CreateClient();

        var projectManager =
            await ProjectManagerTestHelper.CreateAndLoginAsync(
                adminClient,
                projectManagerClient,
                _factory.Services);

        AuthenticationTestHelper.SetBearerToken(
            projectManagerClient,
            projectManager.Authentication.AccessToken);

        var request =
            TestProjectFactory.CreateInvalidDateRequest();

        var response =
            await projectManagerClient.PostAsJsonAsync(
                "/api/projects",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProjects_WithoutToken_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();

        var response =
            await client.GetAsync(
                "/api/projects");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}