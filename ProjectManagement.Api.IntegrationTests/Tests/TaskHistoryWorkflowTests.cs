using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;


[Collection(ApiTestCollection.Name)]
public sealed class TaskHistoryWorkflowTests
{
    private readonly ProjectManagementApiFactory _factory;

    public TaskHistoryWorkflowTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHistories_AsProjectOwner_ReturnsSuccess()
    {
        using var context =
            await TaskHistoryTestHelper.CreateContextAsync(
                _factory);

        var histories =
            await TaskHistoryTestHelper.GetHistoriesAsync(
                context.ProjectManagerClient,
                context.Task.Id);

        histories.Should().NotBeNull();

        histories
            .Should()
            .OnlyContain(history =>
                history.TaskId == context.Task.Id);
    }

    [Fact]
    public async Task ChangeStatus_CreatesHistoryRecord()
    {
        using var context =
            await TaskHistoryTestHelper.CreateContextAsync(
                _factory);

        var historiesBefore =
            await TaskHistoryTestHelper.GetHistoriesAsync(
                context.ProjectManagerClient,
                context.Task.Id);

        var request = new
        {
            status = "InProgress"
        };

        var updateResponse =
            await context.TeamMemberClient.PatchAsJsonAsync(
                $"/api/tasks/{context.Task.Id}/status",
                request);

        var updateResponseText =
            await updateResponse.Content.ReadAsStringAsync();

        updateResponse.StatusCode
            .Should()
            .Be(
                HttpStatusCode.OK,
                $"durum güncellemesi başarılı olmalıydı. " +
                $"Response: {updateResponseText}");

        var historiesAfter =
            await TaskHistoryTestHelper.GetHistoriesAsync(
                context.ProjectManagerClient,
                context.Task.Id);

        historiesAfter.Count
            .Should()
            .BeGreaterThan(historiesBefore.Count);

        historiesAfter
            .Should()
            .Contain(history =>
                history.TaskId == context.Task.Id &&
                history.NewValue == "InProgress");
    }

    [Fact]
    public async Task ChangeStatus_ToDone_CreatesDoneHistory()
    {
        using var context =
            await TaskHistoryTestHelper.CreateContextAsync(
                _factory);

        var response =
            await context.TeamMemberClient.PatchAsJsonAsync(
                $"/api/tasks/{context.Task.Id}/status",
                new
                {
                    status = "Done"
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var histories =
            await TaskHistoryTestHelper.GetHistoriesAsync(
                context.TeamMemberClient,
                context.Task.Id);

        histories.Should().Contain(history =>
            history.NewValue == "Done" &&
            history.ChangedByUserId ==
            context.TeamMemberAuthentication.User.Id);
    }

    [Fact]
    public async Task ChangeAssignment_CreatesHistoryRecord()
    {
        using var context =
            await TaskHistoryTestHelper.CreateContextAsync(
                _factory);

        var assignResponse =
            await context.ProjectManagerClient.PatchAsJsonAsync(
                $"/api/tasks/{context.Task.Id}/assign",
                new
                {
                    assignedToUserId = (int?)null
                });

        var assignResponseText =
            await assignResponse.Content.ReadAsStringAsync();

        assignResponse.StatusCode
            .Should()
            .Be(
                HttpStatusCode.OK,
                $"görev ataması kaldırılabilmeliydi. " +
                $"Response: {assignResponseText}");

        var histories =
            await TaskHistoryTestHelper.GetHistoriesAsync(
                context.ProjectManagerClient,
                context.Task.Id);

        histories
            .Should()
            .Contain(history =>
                history.TaskId == context.Task.Id &&
                history.ChangedByUserId ==
                context.ProjectManager.User.Id);
    }

   [Fact]
    public async Task UpdateTask_CreatesHistoryRecords()
    {
        using var context =
            await TaskHistoryTestHelper.CreateContextAsync(
                _factory);

        var historiesBefore =
            await TaskHistoryTestHelper.GetHistoriesAsync(
                context.ProjectManagerClient,
                context.Task.Id);

        var updatedTitle =
            $"Updated Task {Guid.NewGuid():N}";

        var request = new
        {
            title = updatedTitle,

            description =
                "Görev geçmişi testi için güncellendi.",

            assignedToUserId =
                context.TeamMemberAuthentication.User.Id,

            status = "InProgress",
            priority = "Critical",
            dueDate = DateTime.UtcNow.AddDays(10),
            estimatedHours = 12m
        };

        var response =
            await context.ProjectManagerClient.PutAsJsonAsync(
                $"/api/tasks/{context.Task.Id}",
                request);

        var responseText =
            await response.Content.ReadAsStringAsync();

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.OK,
                $"görev güncellenebilmeliydi. " +
                $"Response: {responseText}");

        var historiesAfter =
            await TaskHistoryTestHelper.GetHistoriesAsync(
                context.ProjectManagerClient,
                context.Task.Id);

        historiesAfter.Count
            .Should()
            .BeGreaterThan(historiesBefore.Count);

        historiesAfter
            .Should()
            .Contain(history =>
                history.TaskId == context.Task.Id &&
                history.NewValue == "InProgress");

        historiesAfter
            .Should()
            .Contain(history =>
                history.TaskId == context.Task.Id &&
                history.NewValue == "Critical");

        historiesAfter
            .Should()
            .Contain(history =>
                history.TaskId == context.Task.Id &&
                history.Description ==
                "Görevin genel bilgileri güncellendi.");
    }
    [Fact]
    public async Task ActiveProjectMember_CanReadHistories()
    {
        using var context =
            await TaskHistoryTestHelper.CreateContextAsync(
                _factory);

        await context.ProjectManagerClient.PatchAsJsonAsync(
            $"/api/tasks/{context.Task.Id}/status",
            new
            {
                status = "InProgress"
            });

        var response =
            await context.TeamMemberClient.GetAsync(
                $"/api/tasks/{context.Task.Id}/histories");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Viewer_CanReadHistories()
    {
        using var context =
            await TaskHistoryTestHelper.CreateContextAsync(
                _factory,
                projectMemberRole: "Viewer");

        await context.ProjectManagerClient.PatchAsJsonAsync(
            $"/api/tasks/{context.Task.Id}/status",
            new
            {
                status = "InProgress"
            });

        var response =
            await context.TeamMemberClient.GetAsync(
                $"/api/tasks/{context.Task.Id}/histories");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemovedMember_CannotReadHistories()
    {
        using var context =
            await TaskHistoryTestHelper.CreateContextAsync(
                _factory);

        var memberUserId =
            context.TeamMemberAuthentication.User.Id;

        var removeResponse =
            await context.ProjectManagerClient.DeleteAsync(
                $"/api/projects/{context.Project.Id}/members/{memberUserId}");

        removeResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var response =
            await context.TeamMemberClient.GetAsync(
                $"/api/tasks/{context.Task.Id}/histories");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetHistories_ForUnknownTask_ReturnsNotFound()
    {
        using var context =
            await TaskHistoryTestHelper.CreateContextAsync(
                _factory);

        var response =
            await context.ProjectManagerClient.GetAsync(
                "/api/tasks/999999999/histories");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetHistories_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var context =
            await TaskHistoryTestHelper.CreateContextAsync(
                _factory);

        using var anonymousClient =
            _factory.CreateClient();

        var response =
            await anonymousClient.GetAsync(
                $"/api/tasks/{context.Task.Id}/histories");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}