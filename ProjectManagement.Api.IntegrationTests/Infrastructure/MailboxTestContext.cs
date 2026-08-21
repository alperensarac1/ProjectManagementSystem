using ProjectManagement.Api.IntegrationTests.Models;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;

public sealed class MailboxTestContext : IDisposable
{
    public required HttpClient SenderClient { get; init; }

    public required HttpClient RecipientClient { get; init; }

    public required HttpClient UnrelatedUserClient { get; init; }

    public required AuthResponseModel SenderAuthentication
    { get; init; }

    public required AuthResponseModel RecipientAuthentication
    { get; init; }

    public required AuthResponseModel UnrelatedUserAuthentication
    { get; init; }

    public void Dispose()
    {
        SenderClient.Dispose();
        RecipientClient.Dispose();
        UnrelatedUserClient.Dispose();
    }
}