using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Models;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;


public static class TaskTimeLogTestHelper
{

    public static async Task<TaskTimeLogTestContext>
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

        return new TaskTimeLogTestContext(
            taskContext,
            task);
    }

    public static async Task<TaskTimeLogResponseModel>
        CreateTimeLogAsync(
            HttpClient client,
            int taskId,
            decimal hours = 2.5m,
            string? description = null,
            DateTime? workDate = null,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        var request = new
        {
            hours,

            description =
                description ??
                $"Integration time log {Guid.NewGuid():N}",

            workDate =
                workDate ??
                DateTime.UtcNow.AddDays(-1)
        };

        var response =
            await client.PostAsJsonAsync(
                $"/api/tasks/{taskId}/time-logs",
                request,
                cancellationToken);

        var responseText =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.Created,
                $"zaman kaydı oluşturma başarılı olmalıydı. " +
                $"Status: {(int)response.StatusCode}, " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<TaskTimeLogResponseModel>>(
                cancellationToken: cancellationToken);

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        return body.Data!;
    }
}

public sealed record TaskTimeLogTestContext(
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