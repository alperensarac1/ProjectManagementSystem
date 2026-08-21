using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Models;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;



public static class TaskHistoryTestHelper
{
   
    public static async Task<TaskHistoryTestContext>
        CreateContextAsync(
            ProjectManagementApiFactory factory,
            string projectMemberRole = "Member",
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var taskContext =
            await ProjectTaskTestHelper.CreateContextAsync(
                factory,
                projectMemberRole,
                cancellationToken);

        int? assignedToUserId =
            string.Equals(
                projectMemberRole,
                "Viewer",
                StringComparison.OrdinalIgnoreCase)
                ? null
                : taskContext.TeamMemberAuthentication.User.Id;

        var task =
            await ProjectTaskTestHelper.CreateTaskAsync(
                taskContext.ProjectManagerClient,
                taskContext.Project.Id,
                assignedToUserId,
                cancellationToken: cancellationToken);

        return new TaskHistoryTestContext(
            taskContext,
            task);
    }

    public static async Task<
        IReadOnlyCollection<TaskHistoryResponseModel>>
        GetHistoriesAsync(
            HttpClient client,
            int taskId,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        var response =
            await client.GetAsync(
                $"/api/tasks/{taskId}/histories",
                cancellationToken);

        var responseText =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.OK,
                $"görev geçmişi alınabilmeliydi. " +
                $"Status: {(int)response.StatusCode}, " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    IReadOnlyCollection<TaskHistoryResponseModel>>>(
                cancellationToken: cancellationToken);

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        return body.Data!;
    }
}

public sealed record TaskHistoryTestContext(
    ProjectTaskTestContext TaskContext,
    TaskResponseModel Task)
    : IDisposable
{
    public HttpClient AdminClient =>
        TaskContext.AdminClient;

    public HttpClient ProjectManagerClient =>
        TaskContext.ProjectManagerClient;

    public HttpClient TeamMemberClient =>
        TaskContext.TeamMemberClient;

    public CreatedProjectManagerResult ProjectManager =>
        TaskContext.ProjectManager;

    public ProjectResponseModel Project =>
        TaskContext.Project;

    public AuthResponseModel TeamMemberAuthentication =>
        TaskContext.TeamMemberAuthentication;

    public void Dispose()
    {
        TaskContext.Dispose();
    }
}