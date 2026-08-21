namespace ProjectManagement.Api.IntegrationTests.Models;

public sealed class ApiResponseModel<T>
{

    public bool Success { get; init; }


    public string Message { get; init; } = string.Empty;

   
    public T? Data { get; init; }

    public Dictionary<string, string[]>? Errors { get; init; }
}