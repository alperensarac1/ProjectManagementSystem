using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Models;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;


public static class ProjectMemberTestHelper
{
    public static async Task<ProjectMemberTestContext>
        CreateContextAsync(
            ProjectManagementApiFactory factory,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var adminClient =
            factory.CreateClient();

        var projectManagerClient =
            factory.CreateClient();

        var teamMemberClient =
            factory.CreateClient();

        var projectManager =
            await ProjectManagerTestHelper.CreateAndLoginAsync(
                adminClient,
                projectManagerClient,
                factory.Services,
                cancellationToken);

        AuthenticationTestHelper.SetBearerToken(
            projectManagerClient,
            projectManager.Authentication.AccessToken);

     
        var projectRequest =
            TestProjectFactory.CreateValidRequest();

        var projectResponse =
            await projectManagerClient.PostAsJsonAsync(
                "/api/projects",
                projectRequest,
                cancellationToken);

        projectResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var projectBody =
            await projectResponse.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<ProjectResponseModel>>(
                    cancellationToken: cancellationToken);

        projectBody.Should().NotBeNull();
        projectBody!.Data.Should().NotBeNull();

        var teamMemberRequest =
            TestUserFactory.CreateRegisterRequest();

        var teamMemberAuthentication =
            await AuthenticationTestHelper.RegisterAsync(
                teamMemberClient,
                teamMemberRequest);

        return new ProjectMemberTestContext(
            adminClient,
            projectManagerClient,
            teamMemberClient,
            projectManager,
            projectBody.Data!,
            teamMemberRequest,
            teamMemberAuthentication);
    }

    public static async Task<ProjectMemberResponseModel> AddMemberAsync(
        HttpClient projectManagerClient,
        int projectId,
        int userId,
        string projectRole = "Member",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            projectManagerClient);

        var request = new AddProjectMemberTestRequest
        {
            UserId = userId,
            Role = projectRole
        };

        var response =
            await projectManagerClient.PostAsJsonAsync(
                $"/api/projects/{projectId}/members",
                request,
                cancellationToken);

        var responseText =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.Created,
                $"üyelik oluşturma başarılı olmalıydı. " +
                $"Status: {(int)response.StatusCode}, " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<ProjectMemberResponseModel>>(
                cancellationToken: cancellationToken);

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        return body.Data!;
    }
}

public sealed record ProjectMemberTestContext(
    HttpClient AdminClient,
    HttpClient ProjectManagerClient,
    HttpClient TeamMemberClient,
    CreatedProjectManagerResult ProjectManager,
    ProjectResponseModel Project,
    RegisterTestRequest TeamMemberRequest,
    AuthResponseModel TeamMemberAuthentication)
    : IDisposable
{
    public void Dispose()
    {
        AdminClient.Dispose();
        ProjectManagerClient.Dispose();
        TeamMemberClient.Dispose();
    }
}