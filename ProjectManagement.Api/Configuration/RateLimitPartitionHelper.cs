using System.Security.Claims;

namespace ProjectManagement.Api.Configuration;

public static class RateLimitPartitionHelper
{
    public static string GetPartitionKey(
        HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

     
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId =
                httpContext.User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"user:{userId}";
            }
        }

    
        var remoteIp =
            httpContext.Connection
                .RemoteIpAddress?
                .ToString();

        return string.IsNullOrWhiteSpace(remoteIp)
            ? "anonymous:unknown"
            : $"ip:{remoteIp}";
    }
}