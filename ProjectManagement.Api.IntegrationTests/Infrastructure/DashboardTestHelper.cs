using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Models;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;

public static class DashboardTestHelper
{
    public static async Task<DashboardTestContext>
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

        return new DashboardTestContext(
            taskContext);
    }


    public static async Task<TaskResponseModel> CreateTaskAsync(
        HttpClient projectManagerClient,
        int projectId,
        string status,
        decimal estimatedHours,
        int? assignedToUserId = null,
        string priority = "Medium",
        DateTime? dueDate = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            projectManagerClient);

        var request = new
        {
            projectId,

            title =
                $"Dashboard Task {Guid.NewGuid():N}",

            description =
                "Dashboard integration testi için oluşturuldu.",

            assignedToUserId,
            status,
            priority,

            dueDate =
                dueDate ?? DateTime.UtcNow.AddDays(7),

            estimatedHours
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
                $"dashboard görevi oluşturulabilmeliydi. " +
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

    public static async Task<DashboardSummaryModel>
        GetSummaryAsync(
            HttpClient client,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        var response =
            await client.GetAsync(
                "/api/dashboard/summary",
                cancellationToken);

        var responseText =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.OK,
                $"dashboard özeti alınabilmeliydi. " +
                $"Status: {(int)response.StatusCode}, " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<DashboardSummaryModel>>(
                cancellationToken: cancellationToken);

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        return body.Data!;
    }
}

public sealed record DashboardTestContext(
    ProjectTaskTestContext TaskContext)
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