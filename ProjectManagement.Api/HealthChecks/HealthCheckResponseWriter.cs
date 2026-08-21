using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProjectManagement.Api.HealthChecks;

public static class HealthCheckResponseWriter
{
    public static async Task WriteResponseAsync(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType =
            "application/json; charset=utf-8";

        var response = new
        {
            status = report.Status.ToString(),

            totalDurationMilliseconds =
                Math.Round(
                    report.TotalDuration.TotalMilliseconds,
                    2),

            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,

                durationMilliseconds =
                    Math.Round(
                        entry.Value.Duration.TotalMilliseconds,
                        2),

                error = entry.Value.Exception?.Message
            }),

            checkedAtUtc = DateTime.UtcNow
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase,

            WriteIndented = true
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(
                response,
                jsonOptions));
    }
}