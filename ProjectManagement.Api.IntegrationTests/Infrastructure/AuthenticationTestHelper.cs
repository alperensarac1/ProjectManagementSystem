using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Models;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;


public static class AuthenticationTestHelper
{

    public static async Task<AuthResponseModel> RegisterAsync(
        HttpClient client,
        RegisterTestRequest request)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        var responseText =
            await response.Content.ReadAsStringAsync();

        response.IsSuccessStatusCode
            .Should()
            .BeTrue(
                $"register isteği başarılı olmalıydı. " +
                $"Status: {(int)response.StatusCode}, " +
                $"Response: {responseText}");

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<AuthResponseModel>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        body.Data!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();

        body.Data.RefreshToken
            .Should()
            .NotBeNullOrWhiteSpace();

        return body.Data;
    }


    public static async Task<AuthResponseModel> LoginAsync(
        HttpClient client,
        string email,
        string password)
    {
        ArgumentNullException.ThrowIfNull(client);

        var request = new
        {
            email,
            password
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        var responseText =
            await response.Content.ReadAsStringAsync();

        response.IsSuccessStatusCode
            .Should()
            .BeTrue(
                $"login isteği başarılı olmalıydı. " +
                $"Status: {(int)response.StatusCode}, " +
                $"Response: {responseText}");

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<AuthResponseModel>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        body.Data!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();

        body.Data.RefreshToken
            .Should()
            .NotBeNullOrWhiteSpace();

        return body.Data;
    }

    public static async Task<AuthResponseModel> RefreshAsync(
        HttpClient client,
        string refreshToken)
    {
        ArgumentNullException.ThrowIfNull(client);

        var request = new
        {
            refreshToken
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                request);

        var responseText =
            await response.Content.ReadAsStringAsync();

        response.IsSuccessStatusCode
            .Should()
            .BeTrue(
                $"refresh isteği başarılı olmalıydı. " +
                $"Status: {(int)response.StatusCode}, " +
                $"Response: {responseText}");

        var body =
            await response.Content
                .ReadFromJsonAsync<
                    ApiResponseModel<AuthResponseModel>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        return body.Data!;
    }

    public static void SetBearerToken(
        HttpClient client,
        string accessToken)
    {
        ArgumentNullException.ThrowIfNull(client);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);
    }


    public static void ClearBearerToken(
        HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        client.DefaultRequestHeaders.Authorization = null;
    }

    public static async Task<AuthResponseModel>
        LoginAsAdminAsync(
            HttpClient client,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var adminAccount =
            await TestAdminSeeder.SeedAsync(
                serviceProvider,
                cancellationToken);

        return await LoginAsync(
            client,
            adminAccount.Email,
            adminAccount.Password);
    }
}