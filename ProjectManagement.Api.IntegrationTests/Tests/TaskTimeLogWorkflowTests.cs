using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;


[Collection(ApiTestCollection.Name)]
public sealed class TaskTimeLogWorkflowTests
{
    private readonly ProjectManagementApiFactory _factory;

    public TaskTimeLogWorkflowTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateTimeLog_AsActiveProjectMember_ReturnsCreated()
    {

        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory);

        const decimal hours = 3.5m;
        const string description =
            "API geliştirme çalışması";

        var timeLog =
            await TaskTimeLogTestHelper.CreateTimeLogAsync(
                context.TeamMemberClient,
                context.Task.Id,
                hours,
                description);

        timeLog.Id.Should().BeGreaterThan(0);
        timeLog.TaskId.Should().Be(context.Task.Id);

        timeLog.UserId.Should().Be(
            context.TeamMemberAuthentication.User.Id);

        timeLog.Hours.Should().Be(hours);
        timeLog.Description.Should().Be(description);

        timeLog.CanEdit.Should().BeTrue();
        timeLog.CanDelete.Should().BeTrue();
    }

    [Fact]
    public async Task GetTimeLogs_ReturnsCreatedRecords()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory);

        var firstTimeLog =
            await TaskTimeLogTestHelper.CreateTimeLogAsync(
                context.TeamMemberClient,
                context.Task.Id,
                hours: 2m,
                description: "Birinci çalışma");

        var secondTimeLog =
            await TaskTimeLogTestHelper.CreateTimeLogAsync(
                context.ProjectManagerClient,
                context.Task.Id,
                hours: 1.5m,
                description: "İkinci çalışma");

        var response =
            await context.TeamMemberClient.GetAsync(
                $"/api/tasks/{context.Task.Id}/time-logs");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    IReadOnlyCollection<TaskTimeLogResponseModel>>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        body.Data!
            .Select(timeLog => timeLog.Id)
            .Should()
            .Contain(
                new[]
                {
                    firstTimeLog.Id,
                    secondTimeLog.Id
                });
    }

    [Fact]
    public async Task UpdateTimeLog_AsOwner_ReturnsUpdatedRecord()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory);

        var timeLog =
            await TaskTimeLogTestHelper.CreateTimeLogAsync(
                context.TeamMemberClient,
                context.Task.Id,
                hours: 2m,
                description: "Eski açıklama");

        var request = new
        {
            hours = 4.25m,
            description = "Güncellenmiş çalışma açıklaması",
            workDate = DateTime.UtcNow.AddDays(-2)
        };

        var response =
            await context.TeamMemberClient.PutAsJsonAsync(
                $"/api/tasks/{context.Task.Id}/time-logs/{timeLog.Id}",
                request);

        var responseText =
            await response.Content.ReadAsStringAsync();

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.OK,
                $"zaman kaydı güncellenebilmeliydi. " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<TaskTimeLogResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.Hours
            .Should()
            .Be(request.hours);

        body.Data.Description
            .Should()
            .Be(request.description);

        body.Data.UserId
            .Should()
            .Be(context.TeamMemberAuthentication.User.Id);
    }

    [Fact]
    public async Task UpdateTimeLog_AsDifferentUser_ReturnsForbidden()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory);

        var timeLog =
            await TaskTimeLogTestHelper.CreateTimeLogAsync(
                context.TeamMemberClient,
                context.Task.Id,
                hours: 2m);

        var request = new
        {
            hours = 5m,
            description =
                "ProjectManager başka kullanıcının kaydını değiştiriyor",
            workDate = DateTime.UtcNow.AddDays(-1)
        };

        var response =
            await context.ProjectManagerClient.PutAsJsonAsync(
                $"/api/tasks/{context.Task.Id}/time-logs/{timeLog.Id}",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteTimeLog_AsOwner_RemovesRecord()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory);

        var timeLog =
            await TaskTimeLogTestHelper.CreateTimeLogAsync(
                context.TeamMemberClient,
                context.Task.Id);

        var deleteResponse =
            await context.TeamMemberClient.DeleteAsync(
                $"/api/tasks/{context.Task.Id}/time-logs/{timeLog.Id}");

        deleteResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var listResponse =
            await context.TeamMemberClient.GetAsync(
                $"/api/tasks/{context.Task.Id}/time-logs");

        listResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await listResponse.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    IReadOnlyCollection<TaskTimeLogResponseModel>>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!
            .Select(item => item.Id)
            .Should()
            .NotContain(timeLog.Id);
    }

    [Fact]
    public async Task DeleteTimeLog_AsProjectOwner_RemovesMemberRecord()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory);

        var timeLog =
            await TaskTimeLogTestHelper.CreateTimeLogAsync(
                context.TeamMemberClient,
                context.Task.Id,
                hours: 2.75m);

        var response =
            await context.ProjectManagerClient.DeleteAsync(
                $"/api/tasks/{context.Task.Id}/time-logs/{timeLog.Id}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Viewer_CannotCreateTimeLog()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory,
                projectMemberRole: "Viewer");

        var request = new
        {
            hours = 2m,
            description =
                "Viewer zaman kaydı oluşturamaz",
            workDate = DateTime.UtcNow.AddDays(-1)
        };

        var response =
            await context.TeamMemberClient.PostAsJsonAsync(
                $"/api/tasks/{context.Task.Id}/time-logs",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Viewer_CanReadTimeLogs()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory,
                projectMemberRole: "Viewer");

        await TaskTimeLogTestHelper.CreateTimeLogAsync(
            context.ProjectManagerClient,
            context.Task.Id,
            hours: 1.25m);

        var response =
            await context.TeamMemberClient.GetAsync(
                $"/api/tasks/{context.Task.Id}/time-logs");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSummary_ReturnsCorrectTotals()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory);

        await TaskTimeLogTestHelper.CreateTimeLogAsync(
            context.TeamMemberClient,
            context.Task.Id,
            hours: 2.5m,
            description: "Frontend çalışması");

        await TaskTimeLogTestHelper.CreateTimeLogAsync(
            context.ProjectManagerClient,
            context.Task.Id,
            hours: 3.25m,
            description: "Backend çalışması");

     
        var response =
            await context.ProjectManagerClient.GetAsync(
                $"/api/tasks/{context.Task.Id}/time-logs/summary");

        var responseText =
            await response.Content.ReadAsStringAsync();

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.OK,
                $"zaman özeti alınabilmeliydi. " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<TaskTimeLogSummaryModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.TaskId
            .Should()
            .Be(context.Task.Id);

        body.Data.ActualHours
            .Should()
            .Be(5.75m);

        body.Data.TimeLogCount
            .Should()
            .Be(2);

        body.Data.ContributorCount
            .Should()
            .Be(2);

        body.Data.EstimatedHours
            .Should()
            .Be(context.Task.EstimatedHours);

        if (context.Task.EstimatedHours.HasValue)
        {
            var expectedDifference =
            body.Data.ActualHours -
            context.Task.EstimatedHours.Value;

            body.Data.DifferenceHours
            .Should()
            .Be(expectedDifference);
        }
    }

    [Fact]
    public async Task TaskResponse_ActualHoursReflectsTimeLogs()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory);

        await TaskTimeLogTestHelper.CreateTimeLogAsync(
            context.TeamMemberClient,
            context.Task.Id,
            hours: 2m);

        await TaskTimeLogTestHelper.CreateTimeLogAsync(
            context.ProjectManagerClient,
            context.Task.Id,
            hours: 4m);

        var response =
            await context.ProjectManagerClient.GetAsync(
                $"/api/tasks/{context.Task.Id}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<TaskResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.ActualHours
            .Should()
            .Be(6m);
    }

    [Fact]
    public async Task RemovedMember_CannotReadTimeLogs()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory);

        await TaskTimeLogTestHelper.CreateTimeLogAsync(
            context.ProjectManagerClient,
            context.Task.Id);

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
                $"/api/tasks/{context.Task.Id}/time-logs");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTimeLog_WithFutureWorkDate_ReturnsBadRequest()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory);

        var request = new
        {
            hours = 2m,
            description =
                "Gelecek tarihli çalışma",
            workDate =
                DateTime.UtcNow.AddDays(1)
        };

        var response =
            await context.TeamMemberClient.PostAsJsonAsync(
                $"/api/tasks/{context.Task.Id}/time-logs",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTimeLog_WithZeroHours_ReturnsBadRequest()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory);

        var request = new
        {
            hours = 0m,
            description =
                "Geçersiz çalışma süresi",
            workDate =
                DateTime.UtcNow.AddDays(-1)
        };

        var response =
            await context.TeamMemberClient.PostAsJsonAsync(
                $"/api/tasks/{context.Task.Id}/time-logs",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<object>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeFalse();

        body.Errors
            .Should()
            .NotBeNull();

        body.Errors!
            .Should()
            .ContainKey("Hours");
    }

    [Fact]
    public async Task UpdateTimeLog_WithWrongTaskId_ReturnsNotFound()
    {
        using var context =
            await TaskTimeLogTestHelper.CreateContextAsync(
                _factory);

        var timeLog =
            await TaskTimeLogTestHelper.CreateTimeLogAsync(
                context.TeamMemberClient,
                context.Task.Id);

        var request = new
        {
            hours = 3m,
            description =
                "Yanlış görev üzerinden güncelleme",
            workDate =
                DateTime.UtcNow.AddDays(-1)
        };

        var response =
            await context.TeamMemberClient.PutAsJsonAsync(
                $"/api/tasks/{context.Task.Id + 99999}/time-logs/{timeLog.Id}",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }
}