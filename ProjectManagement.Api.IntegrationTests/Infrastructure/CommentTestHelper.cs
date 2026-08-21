using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Models;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;

public static class CommentTestHelper
{

    public static async Task<CommentTestContext>
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

        var assignedToUserId =
        string.Equals(
            projectMemberRole,
            "Viewer",
            StringComparison.OrdinalIgnoreCase)
            ? (int?)null
            : taskContext.TeamMemberAuthentication.User.Id;
        var task =
            await ProjectTaskTestHelper.CreateTaskAsync(
                taskContext.ProjectManagerClient,
                taskContext.Project.Id,
                assignedToUserId,
                cancellationToken: cancellationToken);

        return new CommentTestContext(
            taskContext,
            task);
    }

    public static async Task<CommentResponseModel>
        CreateCommentAsync(
            HttpClient client,
            int taskId,
            string? content = null,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        var request = new
        {
            content =
                content ??
                $"Integration comment {Guid.NewGuid():N}"
        };

        var response =
            await client.PostAsJsonAsync(
                $"/api/tasks/{taskId}/comments",
                request,
                cancellationToken);

        var responseText =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.Created,
                $"yorum oluşturma başarılı olmalıydı. " +
                $"Status: {(int)response.StatusCode}, " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<CommentResponseModel>>(
                cancellationToken: cancellationToken);

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        return body.Data!;
    }
}

public sealed record CommentTestContext(
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