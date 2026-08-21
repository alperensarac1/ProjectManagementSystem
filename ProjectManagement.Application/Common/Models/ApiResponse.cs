namespace ProjectManagement.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }

    public static ApiResponse<T> Succeed(
        T data,
        string message = "İşlem başarıyla tamamlandı.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> Succeed(
        string message = "İşlem başarıyla tamamlandı.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = default,
            Errors = null
        };
    }

    public static ApiResponse<T> Fail(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors
        };
    }
}