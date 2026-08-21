using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Models;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;


public static class ProjectTaskTestHelper
{

    public static async Task<ProjectTaskTestContext>
        CreateContextAsync(
            ProjectManagementApiFactory factory,
            string projectMemberRole = "Member",
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var memberContext =
            await ProjectMemberTestHelper.CreateContextAsync(
                factory,
                cancellationToken);

        AuthenticationTestHelper.SetBearerToken(
            memberContext.ProjectManagerClient,
            memberContext.ProjectManager
                .Authentication.AccessToken);

        var membership =
            await ProjectMemberTestHelper.AddMemberAsync(
                memberContext.ProjectManagerClient,
                memberContext.Project.Id,
                memberContext.TeamMemberAuthentication.User.Id,
                projectMemberRole,
                cancellationToken);

        AuthenticationTestHelper.SetBearerToken(
            memberContext.TeamMemberClient,
            memberContext.TeamMemberAuthentication.AccessToken);

        return new ProjectTaskTestContext(
            memberContext,
            membership);
    }

    public static async Task<TaskResponseModel> CreateTaskAsync(
        HttpClient projectManagerClient,
        int projectId,
        int? assignedToUserId = null,
        string status = "Todo",
        string priority = "High",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            projectManagerClient);

        var request = new
        {
            projectId,

            title =
                $"Integration Task {Guid.NewGuid():N}",

            description =
                "Integration test tarafından oluşturulan görev.",

            assignedToUserId,

            status,

            priority,

            dueDate =
                DateTime.UtcNow.AddDays(7),

            estimatedHours =
                8.5m
        };

        var response =
            await projectManagerClient.PostAsJsonAsync(
                "/api/tasks",
                request,
                cancellationToken);

        var responseText =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.Created,
                $"görev oluşturma başarılı olmalıydı. " +
                $"Status: {(int)response.StatusCode}, " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<TaskResponseModel>>(
                cancellationToken: cancellationToken);

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        return body.Data!;
    }
}

public sealed record ProjectTaskTestContext(
    ProjectMemberTestContext MemberContext,
    ProjectMemberResponseModel Membership)
    : IDisposable
{
    public HttpClient AdminClient =>
        MemberContext.AdminClient;

    public HttpClient ProjectManagerClient =>
        MemberContext.ProjectManagerClient;

    public HttpClient TeamMemberClient =>
        MemberContext.TeamMemberClient;

    public CreatedProjectManagerResult ProjectManager =>
        MemberContext.ProjectManager;

    public ProjectResponseModel Project =>
        MemberContext.Project;

    public AuthResponseModel TeamMemberAuthentication =>
        MemberContext.TeamMemberAuthentication;

    public void Dispose()
    {
        MemberContext.Dispose();
    }
}