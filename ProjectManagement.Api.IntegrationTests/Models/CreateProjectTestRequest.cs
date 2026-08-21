namespace ProjectManagement.Api.IntegrationTests.Models;

public sealed class CreateProjectTestRequest
{
    public string Name { get; init; } =
        string.Empty;

    public string? Description { get; init; }

    public DateTime StartDate { get; init; }

    public DateTime? EndDate { get; init; }

    public string Status { get; init; } =
        "Planning";

    public int? OwnerId { get; init; }
}