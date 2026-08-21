using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;

[Collection(ApiTestCollection.Name)]
public sealed class ProjectTaskWorkflowTests
{
    private readonly ProjectManagementApiFactory _factory;

    public ProjectTaskWorkflowTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateTask_AsProjectOwner_ReturnsCreatedTask()
    {
        using var context =
            await ProjectTaskTestHelper.CreateContextAsync(
                _factory);

        var memberUserId =
            context.TeamMemberAuthentication.User.Id;

        var task =
            await ProjectTaskTestHelper.CreateTaskAsync(
                context.ProjectManagerClient,
                context.Project.Id,
                memberUserId);

        task.Id.Should().BeGreaterThan(0);
        task.ProjectId.Should().Be(context.Project.Id);
        task.AssignedToUserId.Should().Be(memberUserId);
        task.Status.Should().Be("Todo");
        task.Priority.Should().Be("High");
        task.EstimatedHours.Should().Be(8.5m);
    }

    [Fact]
    public async Task GetTask_AsActiveProjectMember_ReturnsTask()
    {
        using var context =
            await ProjectTaskTestHelper.CreateContextAsync(
                _factory);

        var task =
            await ProjectTaskTestHelper.CreateTaskAsync(
                context.ProjectManagerClient,
                context.Project.Id,
                context.TeamMemberAuthentication.User.Id);

        var response =
            await context.TeamMemberClient.GetAsync(
                $"/api/tasks/{task.Id}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<TaskResponseModel>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data!.Id.Should().Be(task.Id);
    }

    [Fact]
    public async Task AssignedMember_CanChangeOwnTaskStatus()
    {
        using var context =
            await ProjectTaskTestHelper.CreateContextAsync(
                _factory,
                projectMemberRole: "Member");

        var memberUserId =
            context.TeamMemberAuthentication.User.Id;

        var task =
            await ProjectTaskTestHelper.CreateTaskAsync(
                context.ProjectManagerClient,
                context.Project.Id,
                memberUserId);

        var response =
            await context.TeamMemberClient.PatchAsJsonAsync(
                $"/api/tasks/{task.Id}/status",
                new
                {
                    status = "InProgress"
                });

        var responseText =
            await response.Content.ReadAsStringAsync();

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.OK,
                $"atanan üye görev durumunu değiştirebilmeliydi. " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<TaskResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data!.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task UpdateStatus_ToDone_SetsCompletedAt()
    {
        using var context =
            await ProjectTaskTestHelper.CreateContextAsync(
                _factory);

        var memberUserId =
            context.TeamMemberAuthentication.User.Id;

        var task =
            await ProjectTaskTestHelper.CreateTaskAsync(
                context.ProjectManagerClient,
                context.Project.Id,
                memberUserId);

        var response =
            await context.TeamMemberClient.PatchAsJsonAsync(
                $"/api/tasks/{task.Id}/status",
                new
                {
                    status = "Done"
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<TaskResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.Status.Should().Be("Done");

        body.Data.CompletedAt
            .Should()
            .NotBeNull();
    }

    [Fact]
    public async Task Viewer_CannotCreateTask()
    {
        using var context =
            await ProjectTaskTestHelper.CreateContextAsync(
                _factory,
                projectMemberRole: "Viewer");

        var request = new
        {
            projectId = context.Project.Id,
            title = "Viewer oluşturamaz",
            description = "Yetki kontrolü",
            assignedToUserId = (int?)null,
            status = "Todo",
            priority = "Medium",
            dueDate = DateTime.UtcNow.AddDays(5),
            estimatedHours = 2m
        };

        var response =
            await context.TeamMemberClient.PostAsJsonAsync(
                "/api/tasks",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AssignTask_ToNonMember_ReturnsBadRequest()
    {
        using var context =
            await ProjectTaskTestHelper.CreateContextAsync(
                _factory);

        var task =
            await ProjectTaskTestHelper.CreateTaskAsync(
                context.ProjectManagerClient,
                context.Project.Id);

        using var outsiderClient =
            _factory.CreateClient();

        var outsiderRequest =
            TestUserFactory.CreateRegisterRequest();

        var outsiderAuthentication =
            await AuthenticationTestHelper.RegisterAsync(
                outsiderClient,
                outsiderRequest);

        var response =
            await context.ProjectManagerClient.PatchAsJsonAsync(
                $"/api/tasks/{task.Id}/assign",
                new
                {
                    assignedToUserId =
                        outsiderAuthentication.User.Id
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteTask_AsProjectOwner_RemovesTask()
    {
        using var context =
            await ProjectTaskTestHelper.CreateContextAsync(
                _factory);

        var task =
            await ProjectTaskTestHelper.CreateTaskAsync(
                context.ProjectManagerClient,
                context.Project.Id);

        var deleteResponse =
            await context.ProjectManagerClient.DeleteAsync(
                $"/api/tasks/{task.Id}");

        deleteResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var getResponse =
            await context.ProjectManagerClient.GetAsync(
                $"/api/tasks/{task.Id}");

        getResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemovedMember_CannotReadTask()
    {
        using var context =
            await ProjectTaskTestHelper.CreateContextAsync(
                _factory);

        var memberUserId =
            context.TeamMemberAuthentication.User.Id;

        var task =
            await ProjectTaskTestHelper.CreateTaskAsync(
                context.ProjectManagerClient,
                context.Project.Id,
                memberUserId);

        var removeResponse =
            await context.ProjectManagerClient.DeleteAsync(
                $"/api/projects/{context.Project.Id}/members/{memberUserId}");

        removeResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var taskResponse =
            await context.TeamMemberClient.GetAsync(
                $"/api/tasks/{task.Id}");

        taskResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }
}