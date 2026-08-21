using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Models;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;

public static class MailboxTestHelper
{
    public static async Task<MailboxTestContext>
        CreateContextAsync(
            ProjectManagementApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var senderClient =
            factory.CreateClient();

        var recipientClient =
            factory.CreateClient();

        var unrelatedUserClient =
            factory.CreateClient();

        var senderRequest =
            TestUserFactory.CreateRegisterRequest();

        var recipientRequest =
            TestUserFactory.CreateRegisterRequest();

        var unrelatedUserRequest =
            TestUserFactory.CreateRegisterRequest();

        var senderAuthentication =
            await AuthenticationTestHelper.RegisterAsync(
                senderClient,
                senderRequest);

        var recipientAuthentication =
            await AuthenticationTestHelper.RegisterAsync(
                recipientClient,
                recipientRequest);

        var unrelatedUserAuthentication =
            await AuthenticationTestHelper.RegisterAsync(
                unrelatedUserClient,
                unrelatedUserRequest);

        AuthenticationTestHelper.SetBearerToken(
            senderClient,
            senderAuthentication.AccessToken);

        AuthenticationTestHelper.SetBearerToken(
            recipientClient,
            recipientAuthentication.AccessToken);

        AuthenticationTestHelper.SetBearerToken(
            unrelatedUserClient,
            unrelatedUserAuthentication.AccessToken);

        return new MailboxTestContext
        {
            SenderClient = senderClient,
            RecipientClient = recipientClient,
            UnrelatedUserClient = unrelatedUserClient,

            SenderAuthentication =
                senderAuthentication,

            RecipientAuthentication =
                recipientAuthentication,

            UnrelatedUserAuthentication =
                unrelatedUserAuthentication
        };
    }

    
    public static Task<HttpResponseMessage>
        SendMessageWithoutAttachmentAsync(
            HttpClient senderClient,
            int recipientUserId,
            string subject,
            string body)
    {
        return SendMessageAsync(
            senderClient,
            recipientUserId,
            subject,
            body,
            attachments: null);
    }

    public static async Task<HttpResponseMessage>
        SendMessageAsync(
            HttpClient senderClient,
            int recipientUserId,
            string subject,
            string body,
            IReadOnlyCollection<MailboxTestAttachment>?
                attachments)
    {
        ArgumentNullException.ThrowIfNull(senderClient);

        using var form =
            new MultipartFormDataContent();

        form.Add(
            new StringContent(
                recipientUserId.ToString()),
            "RecipientUserIds");

        form.Add(
            new StringContent(subject),
            "Subject");

        form.Add(
            new StringContent(body),
            "Body");

        if (attachments is not null)
        {
            foreach (var attachment in attachments)
            {
                var fileContent =
                    new ByteArrayContent(
                        attachment.Content);

                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        attachment.ContentType);

                form.Add(
                    fileContent,
                    "Attachments",
                    attachment.FileName);
            }
        }

        return await senderClient.PostAsync(
            "/api/mailbox/messages",
            form);
    }

    public static async Task<
        MailboxMessageDetailResponseModel>
        SendAndReadMessageAsync(
            HttpClient senderClient,
            int recipientUserId,
            string? subject = null,
            IReadOnlyCollection<MailboxTestAttachment>?
                attachments = null)
    {
        subject ??=
            $"Mailbox Test {Guid.NewGuid():N}";

        var response =
            await SendMessageAsync(
                senderClient,
                recipientUserId,
                subject,
                "Integration test mailbox mesaj içeriği.",
                attachments);

        var responseText =
            await response.Content.ReadAsStringAsync();

        response.IsSuccessStatusCode
            .Should()
            .BeTrue(
                $"mesaj gönderme başarılı olmalıydı. " +
                $"Status: {(int)response.StatusCode}, " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    MailboxMessageDetailResponseModel>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        return body.Data!;
    }

    public static MailboxTestAttachment CreatePdfAttachment()
    {
        return new MailboxTestAttachment
        {
            FileName =
                $"test-{Guid.NewGuid():N}.pdf",

            ContentType =
                "application/pdf",

            Content =
            [
                0x25,
                0x50,
                0x44,
                0x46,
                0x2D,
                0x31,
                0x2E,
                0x37,
                0x0A,
                0x25,
                0x25,
                0x45,
                0x4F,
                0x46
            ]
        };
    }

    public static MailboxTestAttachment CreatePngAttachment()
    {
        return new MailboxTestAttachment
        {
            FileName =
                $"image-{Guid.NewGuid():N}.png",

            ContentType =
                "image/png",

            Content =
            [
                0x89,
                0x50,
                0x4E,
                0x47,
                0x0D,
                0x0A,
                0x1A,
                0x0A,
                0x00,
                0x00,
                0x00,
                0x00
            ]
        };
    }

    public static MailboxTestAttachment CreateJpegAttachment()
    {
        return new MailboxTestAttachment
        {
            FileName =
                $"image-{Guid.NewGuid():N}.jpg",

            ContentType =
                "image/jpeg",

            Content =
            [
                0xFF,
                0xD8,
                0xFF,
                0xE0,
                0x00,
                0x10,
                0x4A,
                0x46,
                0x49,
                0x46,
                0x00,
                0x01
            ]
        };
    }
}

public sealed class MailboxTestAttachment
{
    public string FileName { get; init; } =
        string.Empty;

    public string ContentType { get; init; } =
        string.Empty;

    public byte[] Content { get; init; } =
        Array.Empty<byte>();
}