namespace ProjectManagement.Api.IntegrationTests.Models;


public sealed class ProjectResponseModel
{
    public int Id { get; init; }

    public string Name { get; init; } =
        string.Empty;

    public string? Description { get; init; }

    public DateTime StartDate { get; init; }

    public DateTime? EndDate { get; init; }


    public string Status { get; init; } =
        string.Empty;

    public int OwnerId { get; init; }

    public string OwnerFullName { get; init; } =
        string.Empty;

    public string OwnerEmail { get; init; } =
        string.Empty;

    public bool IsArchived { get; init; }

    public DateTime? ArchivedAt { get; init; }

    public int MemberCount { get; init; }

    public int TaskCount { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}