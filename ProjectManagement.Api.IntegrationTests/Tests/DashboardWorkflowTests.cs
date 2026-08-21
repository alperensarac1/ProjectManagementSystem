using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;


[Collection(ApiTestCollection.Name)]
public sealed class DashboardWorkflowTests
{
    private readonly ProjectManagementApiFactory _factory;

    public DashboardWorkflowTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSummary_AsProjectManager_ReturnsOwnProject()
    {
        using var context =
            await DashboardTestHelper.CreateContextAsync(
                _factory);

        await DashboardTestHelper.CreateTaskAsync(
            context.ProjectManagerClient,
            context.Project.Id,
            status: "Todo",
            estimatedHours: 5m);

        var summary =
            await DashboardTestHelper.GetSummaryAsync(
                context.ProjectManagerClient);

        summary.TotalProjectCount
            .Should()
            .BeGreaterThanOrEqualTo(1);

        summary.TotalTaskCount
            .Should()
            .BeGreaterThanOrEqualTo(1);

        summary.GeneratedAtUtc
            .Should()
            .NotBe(default);
    }

    [Fact]
    public async Task GetSummary_ReturnsCorrectTaskStatusCounts()
    {
        using var context =
            await DashboardTestHelper.CreateContextAsync(
                _factory);

        await DashboardTestHelper.CreateTaskAsync(
            context.ProjectManagerClient,
            context.Project.Id,
            status: "Todo",
            estimatedHours: 2m);

        await DashboardTestHelper.CreateTaskAsync(
            context.ProjectManagerClient,
            context.Project.Id,
            status: "InProgress",
            estimatedHours: 3m);

        await DashboardTestHelper.CreateTaskAsync(
            context.ProjectManagerClient,
            context.Project.Id,
            status: "Done",
            estimatedHours: 4m);

        var summary =
            await DashboardTestHelper.GetSummaryAsync(
                context.ProjectManagerClient);

        summary.TotalTaskCount
            .Should()
            .Be(3);

        summary.TodoTaskCount
            .Should()
            .Be(1);

        summary.InProgressTaskCount
            .Should()
            .Be(1);

        summary.DoneTaskCount
            .Should()
            .Be(1);

        summary.TotalEstimatedHours
            .Should()
            .Be(9m);
    }

    [Fact]
    public async Task TeamMember_SeesAssignedTaskCount()
    {
        using var context =
            await DashboardTestHelper.CreateContextAsync(
                _factory);

        var teamMemberId =
            context.TeamMemberAuthentication.User.Id;

        await DashboardTestHelper.CreateTaskAsync(
            context.ProjectManagerClient,
            context.Project.Id,
            status: "Todo",
            estimatedHours: 3m,
            assignedToUserId: teamMemberId);

        await DashboardTestHelper.CreateTaskAsync(
            context.ProjectManagerClient,
            context.Project.Id,
            status: "InProgress",
            estimatedHours: 4m,
            assignedToUserId: teamMemberId);

        await DashboardTestHelper.CreateTaskAsync(
            context.ProjectManagerClient,
            context.Project.Id,
            status: "Todo",
            estimatedHours: 2m);

        var summary =
            await DashboardTestHelper.GetSummaryAsync(
                context.TeamMemberClient);

        summary.TotalProjectCount
            .Should()
            .Be(1);

        summary.TotalTaskCount
            .Should()
            .Be(3);

        summary.MyAssignedTaskCount
            .Should()
            .Be(2);
    }

    [Fact]
    public async Task GetSummary_ReturnsActualAndMyLoggedHours()
    {
        using var context =
            await DashboardTestHelper.CreateContextAsync(
                _factory);

        var teamMemberId =
            context.TeamMemberAuthentication.User.Id;

        var task =
            await DashboardTestHelper.CreateTaskAsync(
                context.ProjectManagerClient,
                context.Project.Id,
                status: "InProgress",
                estimatedHours: 10m,
                assignedToUserId: teamMemberId);

        await TaskTimeLogTestHelper.CreateTimeLogAsync(
            context.TeamMemberClient,
            task.Id,
            hours: 2.5m,
            description: "TeamMember çalışması");

        await TaskTimeLogTestHelper.CreateTimeLogAsync(
            context.ProjectManagerClient,
            task.Id,
            hours: 3m,
            description: "ProjectManager çalışması");

        var memberSummary =
            await DashboardTestHelper.GetSummaryAsync(
                context.TeamMemberClient);

        memberSummary.TotalActualHours
            .Should()
            .Be(5.5m);

        memberSummary.MyLoggedHours
            .Should()
            .Be(2.5m);

        var managerSummary =
            await DashboardTestHelper.GetSummaryAsync(
                context.ProjectManagerClient);

        managerSummary.TotalActualHours
            .Should()
            .Be(5.5m);

        managerSummary.MyLoggedHours
            .Should()
            .Be(3m);
    }

    [Fact]
    public async Task RecentTasks_RespectsCountParameter()
    {
        using var context =
            await DashboardTestHelper.CreateContextAsync(
                _factory);

        await DashboardTestHelper.CreateTaskAsync(
            context.ProjectManagerClient,
            context.Project.Id,
            status: "Todo",
            estimatedHours: 1m);

        await DashboardTestHelper.CreateTaskAsync(
            context.ProjectManagerClient,
            context.Project.Id,
            status: "InProgress",
            estimatedHours: 2m);

        await DashboardTestHelper.CreateTaskAsync(
            context.ProjectManagerClient,
            context.Project.Id,
            status: "Done",
            estimatedHours: 3m);

        var response =
            await context.ProjectManagerClient.GetAsync(
                "/api/dashboard/recent-tasks?count=2");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    IReadOnlyCollection<RecentTaskModel>>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        body.Data!
            .Should()
            .HaveCount(2);

        body.Data
            .Should()
            .OnlyContain(task =>
                task.ProjectId == context.Project.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task RecentTasks_WithInvalidCount_ReturnsBadRequest(
        int count)
    {
        using var context =
            await DashboardTestHelper.CreateContextAsync(
                _factory);

        var response =
            await context.ProjectManagerClient.GetAsync(
                $"/api/dashboard/recent-tasks?count={count}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Viewer_CanReadDashboard()
    {
        using var context =
            await DashboardTestHelper.CreateContextAsync(
                _factory,
                projectMemberRole: "Viewer");

        await DashboardTestHelper.CreateTaskAsync(
            context.ProjectManagerClient,
            context.Project.Id,
            status: "Todo",
            estimatedHours: 2m);

        var response =
            await context.TeamMemberClient.GetAsync(
                "/api/dashboard/summary");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<DashboardSummaryModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.TotalProjectCount
            .Should()
            .Be(1);

        body.Data.TotalTaskCount
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task RemovedMember_NoLongerSeesProjectInDashboard()
    {
        using var context =
            await DashboardTestHelper.CreateContextAsync(
                _factory);

        await DashboardTestHelper.CreateTaskAsync(
            context.ProjectManagerClient,
            context.Project.Id,
            status: "Todo",
            estimatedHours: 2m);

        var memberUserId =
            context.TeamMemberAuthentication.User.Id;

        var removeResponse =
            await context.ProjectManagerClient.DeleteAsync(
                $"/api/projects/{context.Project.Id}/members/{memberUserId}");

        removeResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var summary =
            await DashboardTestHelper.GetSummaryAsync(
                context.TeamMemberClient);

        summary.TotalProjectCount
            .Should()
            .Be(0);

        summary.TotalTaskCount
            .Should()
            .Be(0);

        summary.TotalEstimatedHours
            .Should()
            .Be(0m);

        summary.TotalActualHours
            .Should()
            .Be(0m);
    }

    [Fact]
    public async Task Dashboard_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var anonymousClient =
            _factory.CreateClient();

        var summaryResponse =
            await anonymousClient.GetAsync(
                "/api/dashboard/summary");

        summaryResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);

        var recentTasksResponse =
            await anonymousClient.GetAsync(
                "/api/dashboard/recent-tasks");

        recentTasksResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}