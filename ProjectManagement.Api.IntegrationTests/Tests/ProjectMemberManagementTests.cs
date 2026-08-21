using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;

[Collection(ApiTestCollection.Name)]
public sealed class ProjectMemberManagementTests
{
    private readonly ProjectManagementApiFactory _factory;

    public ProjectMemberManagementTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddMember_AsProjectOwner_ReturnsCreated()
    {
        using var context =
            await ProjectMemberTestHelper.CreateContextAsync(
                _factory);

        var request = new AddProjectMemberTestRequest
        {
            UserId =
                context.TeamMemberAuthentication.User!.Id,

            Role =
                "Contributor"
        };

        var response =
            await context.ProjectManagerClient.PostAsJsonAsync(
                $"/api/projects/{context.Project.Id}/members",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<ProjectMemberResponseModel>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        body.Data!.ProjectId
            .Should()
            .Be(context.Project.Id);

        body.Data.UserId
            .Should()
            .Be(context.TeamMemberAuthentication.User.Id);

        body.Data.ProjectRole
            .Should()
            .Be("Contributor");

        body.Data.SystemRole
            .Should()
            .Be("TeamMember");

        body.Data.IsActive
            .Should()
            .BeTrue();

        body.Data.IsProjectOwner
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task GetMembers_AsProjectOwner_ReturnsAddedMember()
    {
        using var context =
            await ProjectMemberTestHelper.CreateContextAsync(
                _factory);

        var userId =
            context.TeamMemberAuthentication.User!.Id;

        var createdMembership =
            await ProjectMemberTestHelper.AddMemberAsync(
                context.ProjectManagerClient,
                context.Project.Id,
                userId,
                "Member");

        createdMembership.UserId
            .Should()
            .Be(userId);

        var response =
            await context.ProjectManagerClient.GetAsync(
                $"/api/projects/{context.Project.Id}/members");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    IReadOnlyCollection<
                        ProjectMemberResponseModel>>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!
            .Should()
            .Contain(member =>
                member.UserId == userId &&
                member.ProjectRole == "Member" &&
                member.IsActive);
    }
    [Fact]
    public async Task AddSameMemberTwice_ReturnsConflict()
    {
        using var context =
            await ProjectMemberTestHelper.CreateContextAsync(
                _factory);

        var request = new AddProjectMemberTestRequest
        {
            UserId =
                context.TeamMemberAuthentication.User!.Id,

            Role =
                "Member"
        };

        var firstResponse =
            await context.ProjectManagerClient.PostAsJsonAsync(
                $"/api/projects/{context.Project.Id}/members",
                request);

        firstResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var secondResponse =
            await context.ProjectManagerClient.PostAsJsonAsync(
                $"/api/projects/{context.Project.Id}/members",
                request);

        secondResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateMemberRole_AsProjectOwner_ReturnsUpdatedMember()
    {
        using var context =
            await ProjectMemberTestHelper.CreateContextAsync(
                _factory);

        var userId =
            context.TeamMemberAuthentication.User!.Id;

        var createdMembership =
            await ProjectMemberTestHelper.AddMemberAsync(
                context.ProjectManagerClient,
                context.Project.Id,
                userId,
                "Member");

        createdMembership.UserId
            .Should()
            .Be(userId);

        var response =
            await context.ProjectManagerClient.PutAsJsonAsync(
                $"/api/projects/{context.Project.Id}/members/{userId}",
                new UpdateProjectMemberTestRequest
                {
                    Role = "Viewer"
                });

        var responseText =
            await response.Content.ReadAsStringAsync();

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.OK,
                $"üyelik rolü güncellenmeliydi. " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<ProjectMemberResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.UserId
            .Should()
            .Be(userId);

        body.Data.ProjectRole
            .Should()
            .Be("Viewer");
    }

    [Fact]
    public async Task TeamMember_CannotAddAnotherMember()
    {
        using var context =
            await ProjectMemberTestHelper.CreateContextAsync(
                _factory);

        AuthenticationTestHelper.SetBearerToken(
            context.TeamMemberClient,
            context.TeamMemberAuthentication.AccessToken);

        var response =
            await context.TeamMemberClient.PostAsJsonAsync(
                $"/api/projects/{context.Project.Id}/members",
                new AddProjectMemberTestRequest
                {
                    UserId =
                        context.TeamMemberAuthentication.User!.Id,

                    Role =
                        "Member"
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddedMember_CanReadProjectDetails()
    {
        using var context =
            await ProjectMemberTestHelper.CreateContextAsync(
                _factory);

        var userId =
            context.TeamMemberAuthentication.User!.Id;

        var addResponse =
            await context.ProjectManagerClient.PostAsJsonAsync(
                $"/api/projects/{context.Project.Id}/members",
                new AddProjectMemberTestRequest
                {
                    UserId = userId,
                    Role = "Member"
                });

        addResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        AuthenticationTestHelper.SetBearerToken(
            context.TeamMemberClient,
            context.TeamMemberAuthentication.AccessToken);

        var response =
            await context.TeamMemberClient.GetAsync(
                $"/api/projects/{context.Project.Id}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemoveMember_AsProjectOwner_ReturnsSuccess()
    {
        using var context =
            await ProjectMemberTestHelper.CreateContextAsync(
                _factory);

        var userId =
            context.TeamMemberAuthentication.User!.Id;

        await context.ProjectManagerClient.PostAsJsonAsync(
            $"/api/projects/{context.Project.Id}/members",
            new AddProjectMemberTestRequest
            {
                UserId = userId,
                Role = "Member"
            });

        var response =
            await context.ProjectManagerClient.DeleteAsync(
                $"/api/projects/{context.Project.Id}/members/{userId}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemovedMember_CannotReadProjectDetails()
    {
        using var context =
            await ProjectMemberTestHelper.CreateContextAsync(
                _factory);

        var userId =
            context.TeamMemberAuthentication.User!.Id;

        await context.ProjectManagerClient.PostAsJsonAsync(
            $"/api/projects/{context.Project.Id}/members",
            new AddProjectMemberTestRequest
            {
                UserId = userId,
                Role = "Member"
            });

        await context.ProjectManagerClient.DeleteAsync(
            $"/api/projects/{context.Project.Id}/members/{userId}");

        AuthenticationTestHelper.SetBearerToken(
            context.TeamMemberClient,
            context.TeamMemberAuthentication.AccessToken);

        var response =
            await context.TeamMemberClient.GetAsync(
                $"/api/projects/{context.Project.Id}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }
}