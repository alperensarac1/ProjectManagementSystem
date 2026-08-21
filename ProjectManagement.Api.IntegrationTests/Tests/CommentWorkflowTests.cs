using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;


[Collection(ApiTestCollection.Name)]
public sealed class CommentWorkflowTests
{
    private readonly ProjectManagementApiFactory _factory;

    public CommentWorkflowTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateComment_AsActiveProjectMember_ReturnsCreated()
    {
        using var context =
            await CommentTestHelper.CreateContextAsync(
                _factory);

        const string content =
            "TeamMember tarafından yazılan yorum.";

        var comment =
            await CommentTestHelper.CreateCommentAsync(
                context.TeamMemberClient,
                context.Task.Id,
                content);

        comment.Id.Should().BeGreaterThan(0);
        comment.TaskId.Should().Be(context.Task.Id);

        comment.UserId.Should().Be(
            context.TeamMemberAuthentication.User.Id);

        comment.Content.Should().Be(content);
        comment.CanEdit.Should().BeTrue();
        comment.CanDelete.Should().BeTrue();
    }

    [Fact]
    public async Task GetComments_ReturnsCreatedComments()
    {
        using var context =
            await CommentTestHelper.CreateContextAsync(
                _factory);

        var firstComment =
            await CommentTestHelper.CreateCommentAsync(
                context.TeamMemberClient,
                context.Task.Id,
                "Birinci yorum");

        var secondComment =
            await CommentTestHelper.CreateCommentAsync(
                context.ProjectManagerClient,
                context.Task.Id,
                "İkinci yorum");

        var response =
            await context.TeamMemberClient.GetAsync(
                $"/api/tasks/{context.Task.Id}/comments");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    IReadOnlyCollection<CommentResponseModel>>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        body.Data!
            .Select(comment => comment.Id)
            .Should()
            .Contain(
                new[]
                {
                    firstComment.Id,
                    secondComment.Id
                });
    }

    [Fact]
    public async Task UpdateComment_AsOwner_ReturnsUpdatedComment()
    {
        using var context =
            await CommentTestHelper.CreateContextAsync(
                _factory);

        var comment =
            await CommentTestHelper.CreateCommentAsync(
                context.TeamMemberClient,
                context.Task.Id,
                "Eski yorum içeriği");

        var request = new
        {
            content =
                "Güncellenmiş yorum içeriği"
        };


        var response =
            await context.TeamMemberClient.PutAsJsonAsync(
                $"/api/tasks/{context.Task.Id}/comments/{comment.Id}",
                request);


        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<CommentResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.Content
            .Should()
            .Be(request.content);

        body.Data.UserId
            .Should()
            .Be(context.TeamMemberAuthentication.User.Id);
    }

    [Fact]
    public async Task UpdateComment_AsDifferentUser_ReturnsForbidden()
    {
    
        using var context =
            await CommentTestHelper.CreateContextAsync(
                _factory);

        var comment =
            await CommentTestHelper.CreateCommentAsync(
                context.TeamMemberClient,
                context.Task.Id,
                "TeamMember yorumu");

        var request = new
        {
            content =
                "ProjectManager değiştirmeye çalışıyor"
        };

        var response =
            await context.ProjectManagerClient.PutAsJsonAsync(
                $"/api/tasks/{context.Task.Id}/comments/{comment.Id}",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteComment_AsCommentOwner_RemovesComment()
    {
        using var context =
            await CommentTestHelper.CreateContextAsync(
                _factory);

        var comment =
            await CommentTestHelper.CreateCommentAsync(
                context.TeamMemberClient,
                context.Task.Id);

        var deleteResponse =
            await context.TeamMemberClient.DeleteAsync(
                $"/api/tasks/{context.Task.Id}/comments/{comment.Id}");

        deleteResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var listResponse =
            await context.TeamMemberClient.GetAsync(
                $"/api/tasks/{context.Task.Id}/comments");

        listResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await listResponse.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    IReadOnlyCollection<CommentResponseModel>>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!
            .Select(item => item.Id)
            .Should()
            .NotContain(comment.Id);
    }

    [Fact]
    public async Task DeleteComment_AsProjectOwner_RemovesMemberComment()
    {

        using var context =
            await CommentTestHelper.CreateContextAsync(
                _factory);

        var comment =
            await CommentTestHelper.CreateCommentAsync(
                context.TeamMemberClient,
                context.Task.Id,
                "Proje sahibinin silebileceği yorum");


        var response =
            await context.ProjectManagerClient.DeleteAsync(
                $"/api/tasks/{context.Task.Id}/comments/{comment.Id}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Viewer_CannotCreateComment()
    {

        using var context =
            await CommentTestHelper.CreateContextAsync(
                _factory,
                projectMemberRole: "Viewer");

        var request = new
        {
            content =
                "Viewer yorum yazmamalı"
        };

        var response =
            await context.TeamMemberClient.PostAsJsonAsync(
                $"/api/tasks/{context.Task.Id}/comments",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Viewer_CanReadComments()
    {
        using var context =
            await CommentTestHelper.CreateContextAsync(
                _factory,
                projectMemberRole: "Viewer");

        await CommentTestHelper.CreateCommentAsync(
            context.ProjectManagerClient,
            context.Task.Id,
            "Viewer tarafından okunabilir yorum");

        var response =
            await context.TeamMemberClient.GetAsync(
                $"/api/tasks/{context.Task.Id}/comments");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemovedMember_CannotReadComments()
    {
        using var context =
            await CommentTestHelper.CreateContextAsync(
                _factory);

        await CommentTestHelper.CreateCommentAsync(
            context.ProjectManagerClient,
            context.Task.Id);

        var teamMemberUserId =
            context.TeamMemberAuthentication.User.Id;

        var removeResponse =
            await context.ProjectManagerClient.DeleteAsync(
                $"/api/projects/{context.Project.Id}/members/{teamMemberUserId}");

        removeResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var response =
            await context.TeamMemberClient.GetAsync(
                $"/api/tasks/{context.Task.Id}/comments");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateComment_WithEmptyContent_ReturnsBadRequest()
    {
        using var context =
            await CommentTestHelper.CreateContextAsync(
                _factory);

        var request = new
        {
            content = string.Empty
        };

        var response =
            await context.TeamMemberClient.PostAsJsonAsync(
                $"/api/tasks/{context.Task.Id}/comments",
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
            .ContainKey("Content");
    }

    [Fact]
    public async Task UpdateComment_WithWrongTaskId_ReturnsNotFound()
    {
        using var context =
            await CommentTestHelper.CreateContextAsync(
                _factory);

        var comment =
            await CommentTestHelper.CreateCommentAsync(
                context.TeamMemberClient,
                context.Task.Id);

        var request = new
        {
            content =
                "Yanlış görev üzerinden güncelleme"
        };

        var response =
            await context.TeamMemberClient.PutAsJsonAsync(
                $"/api/tasks/{context.Task.Id + 99999}/comments/{comment.Id}",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }
}