using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Models;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;


public static class ProjectManagerTestHelper
{
    public static async Task<CreatedProjectManagerResult>
        CreateAndLoginAsync(
            HttpClient adminClient,
            HttpClient projectManagerClient,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(adminClient);
        ArgumentNullException.ThrowIfNull(projectManagerClient);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var adminAuthentication =
            await AuthenticationTestHelper.LoginAsAdminAsync(
                adminClient,
                serviceProvider,
                cancellationToken);

        AuthenticationTestHelper.SetBearerToken(
            adminClient,
            adminAuthentication.AccessToken);

        var createRequest =
            TestAdminUserFactory.CreateProjectManagerRequest();

        var createResponse =
            await adminClient.PostAsJsonAsync(
                "/api/users",
                createRequest,
                cancellationToken);

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var createBody =
            await createResponse.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<UserResponseModel>>(
                    cancellationToken: cancellationToken);

        createBody.Should().NotBeNull();
        createBody!.Data.Should().NotBeNull();

        var authentication =
            await AuthenticationTestHelper.LoginAsync(
                projectManagerClient,
                createRequest.Email,
                createRequest.Password);

        return new CreatedProjectManagerResult(
            createBody.Data!,
            createRequest,
            authentication);
    }
}

public sealed record CreatedProjectManagerResult(
    UserResponseModel User,
    CreateUserTestRequest Request,
    AuthResponseModel Authentication);