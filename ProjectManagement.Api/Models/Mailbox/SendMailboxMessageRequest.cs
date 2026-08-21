using Microsoft.AspNetCore.Http;

namespace ProjectManagement.Api.Models.Mailbox;

public sealed class SendMailboxMessageRequest
{

    public List<int> RecipientUserIds { get; set; } =
        new();

    public string Subject { get; set; } =
        string.Empty;

    public string Body { get; set; } =
        string.Empty;

    public List<IFormFile> Attachments { get; set; } =
        new();
}