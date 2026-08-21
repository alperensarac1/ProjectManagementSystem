using ProjectManagement.Api.IntegrationTests.Models;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;

public static class TestProjectFactory
{
    public static CreateProjectTestRequest
        CreateValidRequest(
            int? ownerId = null)
    {
        var uniqueValue =
            Guid.NewGuid().ToString("N");

        var startDate =
            DateTime.UtcNow.Date.AddDays(1);

        return new CreateProjectTestRequest
        {
            Name =
                $"Integration Project {uniqueValue}",

            Description =
                "Integration testleri tarafından oluşturulan proje.",

            StartDate =
                startDate,

            EndDate =
                startDate.AddMonths(3),

            Status =
                "Planning",

            OwnerId =
                ownerId
        };
    }

    public static UpdateProjectTestRequest
        CreateUpdateRequest(
            int? ownerId = null)
    {
        var startDate =
            DateTime.UtcNow.Date.AddDays(2);

        return new UpdateProjectTestRequest
        {
            Name =
                "Updated Integration Project",

            Description =
                "Proje bilgileri integration testi tarafından güncellendi.",

            StartDate =
                startDate,

            EndDate =
                startDate.AddMonths(6),

            Status =
                "Active",

            OwnerId =
                ownerId
        };
    }

    public static CreateProjectTestRequest
        CreateInvalidDateRequest(
            int? ownerId = null)
    {
        var startDate =
            DateTime.UtcNow.Date.AddMonths(2);

        return new CreateProjectTestRequest
        {
            Name =
                "Invalid Date Project",

            Description =
                "Bitiş tarihi başlangıç tarihinden önce olan test projesi.",

            StartDate =
                startDate,

            EndDate =
                startDate.AddDays(-10),

            Status =
                "Planning",

            OwnerId =
                ownerId
        };
    }
}