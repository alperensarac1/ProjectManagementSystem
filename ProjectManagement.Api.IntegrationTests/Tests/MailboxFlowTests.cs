using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagement.Api.IntegrationTests.Infrastructure;
using ProjectManagement.Api.IntegrationTests.Models;
using Xunit;

namespace ProjectManagement.Api.IntegrationTests.Tests;

[Collection(ApiTestCollection.Name)]
public sealed class MailboxFlowTests
{
    private readonly ProjectManagementApiFactory _factory;

    public MailboxFlowTests(
        ProjectManagementApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SendMessage_WithoutAttachment_ReturnsCreated()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var subject =
            $"Mailbox Without Attachment {Guid.NewGuid():N}";

        var response =
            await MailboxTestHelper
                .SendMessageWithoutAttachmentAsync(
                    context.SenderClient,
                    context.RecipientAuthentication.User!.Id,
                    subject,
                    "Dosya eki bulunmayan test mesajıdır.");

        var responseText =
            await response.Content.ReadAsStringAsync();

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.Created,
                $"mesaj oluşturulmalıydı. " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    MailboxMessageDetailResponseModel>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        body.Data!.Id
            .Should()
            .BeGreaterThan(0);

        body.Data.Subject
            .Should()
            .Be(subject);

        body.Data.Sender.Id
            .Should()
            .Be(context.SenderAuthentication.User!.Id);

        body.Data.Recipients
            .Should()
            .ContainSingle(recipient =>
                recipient.Id ==
                context.RecipientAuthentication.User!.Id);

        body.Data.Attachments
            .Should()
            .BeEmpty();
    }

    [Fact]
    public async Task SendMessage_WithPdfAttachment_ReturnsCreated()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var pdf =
            MailboxTestHelper.CreatePdfAttachment();

        var createdMessage =
            await MailboxTestHelper.SendAndReadMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id,
                attachments: [pdf]);

        createdMessage.Attachments
            .Should()
            .ContainSingle();

        var attachment =
            createdMessage.Attachments.Single();

        attachment.OriginalFileName
            .Should()
            .Be(pdf.FileName);

        attachment.ContentType
            .Should()
            .Be("application/pdf");

        attachment.Extension
            .Should()
            .Be(".pdf");

        attachment.FileSize
            .Should()
            .Be(pdf.Content.LongLength);

        attachment.IsAvailable
            .Should()
            .BeTrue();

        attachment.IsFileDeleted
            .Should()
            .BeFalse();

        Directory
            .EnumerateFiles(
                _factory.MailboxRootDirectory,
                "*",
                SearchOption.AllDirectories)
            .Should()
            .NotBeEmpty();
    }

    [Fact]
    public async Task SendMessage_WithPngAttachment_ReturnsCreated()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var png =
            MailboxTestHelper.CreatePngAttachment();

        var createdMessage =
            await MailboxTestHelper.SendAndReadMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id,
                attachments: [png]);

        createdMessage.Attachments
            .Should()
            .ContainSingle(attachment =>
                attachment.OriginalFileName == png.FileName &&
                attachment.ContentType == "image/png" &&
                attachment.Extension == ".png" &&
                attachment.IsAvailable);
    }

    [Fact]
    public async Task SendMessage_WithJpegAttachment_ReturnsCreated()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var jpeg =
            MailboxTestHelper.CreateJpegAttachment();

        var createdMessage =
            await MailboxTestHelper.SendAndReadMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id,
                attachments: [jpeg]);

        createdMessage.Attachments
            .Should()
            .ContainSingle(attachment =>
                attachment.OriginalFileName == jpeg.FileName &&
                attachment.ContentType == "image/jpeg" &&
                attachment.Extension == ".jpg" &&
                attachment.IsAvailable);
    }

    [Fact]
    public async Task Recipient_CanSeeMessageInInbox()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var subject =
            $"Inbox Test {Guid.NewGuid():N}";

        var createdMessage =
            await MailboxTestHelper.SendAndReadMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id,
                subject);

        var response =
            await context.RecipientClient.GetAsync(
                "/api/mailbox/inbox?page=1&pageSize=20");

        var responseText =
            await response.Content.ReadAsStringAsync();

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.OK,
                $"gelen kutusu getirilebilmeliydi. " +
                $"Status: {(int)response.StatusCode}, " +
                $"Response: {responseText}");

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    PagedResultModel<
                        MailboxMessageListItemResponseModel>>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();

        body.Data!.Items
            .Should()
            .Contain(message =>
                message.Id == createdMessage.Id &&
                message.Subject == subject &&
                message.Sender.Id ==
                context.SenderAuthentication.User!.Id &&
                message.IsRead == false);
    }

    [Fact]
    public async Task Sender_CanSeeMessageInSentBox()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var subject =
            $"Sent Test {Guid.NewGuid():N}";

        var createdMessage =
            await MailboxTestHelper.SendAndReadMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id,
                subject);

        var response =
            await context.SenderClient.GetAsync(
                "/api/mailbox/sent?page=1&pageSize=20");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    PagedResultModel<
                        MailboxMessageListItemResponseModel>>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        body.Data!.Items
            .Should()
            .Contain(message =>
                message.Id == createdMessage.Id &&
                message.Subject == subject &&
                message.Recipients.Any(recipient =>
                    recipient.Id ==
                    context.RecipientAuthentication.User!.Id));
    }

    [Fact]
    public async Task OpeningMessage_MarksMessageAsRead()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var createdMessage =
            await MailboxTestHelper.SendAndReadMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id);

        var detailResponse =
            await context.RecipientClient.GetAsync(
                $"/api/mailbox/messages/{createdMessage.Id}");

        detailResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var detailBody =
            await detailResponse.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    MailboxMessageDetailResponseModel>>();

        detailBody.Should().NotBeNull();
        detailBody!.Data.Should().NotBeNull();

        detailBody.Data!.IsRead
            .Should()
            .BeTrue();

        detailBody.Data.ReadAtUtc
            .Should()
            .NotBeNull();

        var inboxResponse =
            await context.RecipientClient.GetAsync(
                "/api/mailbox/inbox?page=1&pageSize=100");

        var inboxResponseText =
            await inboxResponse.Content.ReadAsStringAsync();

        inboxResponse.StatusCode
            .Should()
            .Be(
                HttpStatusCode.OK,
                $"gelen kutusu getirilebilmeliydi. " +
                $"Response: {inboxResponseText}");

        var inboxBody =
            await inboxResponse.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    PagedResultModel<
                        MailboxMessageListItemResponseModel>>>();

        inboxBody.Should().NotBeNull();
        inboxBody!.Data.Should().NotBeNull();
        inboxBody!.Data!.Items
            .Single(message =>
                message.Id == createdMessage.Id)
            .IsRead
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task Recipient_CanMarkMessageAsUnread()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var createdMessage =
            await MailboxTestHelper.SendAndReadMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id);

        /*
         * Önce mesajı açarak okundu yapıyoruz.
         */
        await context.RecipientClient.GetAsync(
            $"/api/mailbox/messages/{createdMessage.Id}");

        var response =
            await context.RecipientClient.PatchAsync(
                $"/api/mailbox/messages/{createdMessage.Id}/unread",
                content: null);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var inboxResponse =
            await context.RecipientClient.GetAsync(
                "/api/mailbox/inbox?page=1&pageSize=100");

        var inboxResponseText =
            await inboxResponse.Content.ReadAsStringAsync();

        inboxResponse.StatusCode
            .Should()
            .Be(
                HttpStatusCode.OK,
                $"gelen kutusu getirilebilmeliydi. " +
                $"Response: {inboxResponseText}");

        var inboxBody =
            await inboxResponse.Content.ReadFromJsonAsync<
                ApiResponseModel<
                    PagedResultModel<
                        MailboxMessageListItemResponseModel>>>();

        inboxBody.Should().NotBeNull();
        inboxBody!.Data.Should().NotBeNull();
        
        var inboxMessage =
            inboxBody!.Data!.Items.Single(
                message =>
                    message.Id == createdMessage.Id);

        inboxMessage.IsRead
            .Should()
            .BeFalse();

        inboxMessage.ReadAtUtc
            .Should()
            .BeNull();
    }

    [Fact]
    public async Task UnrelatedUser_CannotReadMessage()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var createdMessage =
            await MailboxTestHelper.SendAndReadMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id);

        var response =
            await context.UnrelatedUserClient.GetAsync(
                $"/api/mailbox/messages/{createdMessage.Id}");

        response.StatusCode
            .Should()
            .BeOneOf(
                HttpStatusCode.Forbidden,
                HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Recipient_CanDownloadPdfAttachment()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var pdf =
            MailboxTestHelper.CreatePdfAttachment();

        var createdMessage =
            await MailboxTestHelper.SendAndReadMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id,
                attachments: [pdf]);

        var attachment =
            createdMessage.Attachments.Single();

        var response =
            await context.RecipientClient.GetAsync(
                $"/api/mailbox/messages/{createdMessage.Id}" +
                $"/attachments/{attachment.Id}/download");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        response.Content.Headers.ContentType?
            .MediaType
            .Should()
            .Be("application/pdf");

        var downloadedBytes =
            await response.Content.ReadAsByteArrayAsync();

        downloadedBytes
            .Should()
            .Equal(pdf.Content);
    }

    [Fact]
    public async Task Recipient_CanDownloadPngAttachment()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var png =
            MailboxTestHelper.CreatePngAttachment();

        var createdMessage =
            await MailboxTestHelper.SendAndReadMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id,
                attachments: [png]);

        var attachment =
            createdMessage.Attachments.Single();

        var response =
            await context.RecipientClient.GetAsync(
                $"/api/mailbox/messages/{createdMessage.Id}" +
                $"/attachments/{attachment.Id}/download");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        response.Content.Headers.ContentType?
            .MediaType
            .Should()
            .Be("image/png");

        var downloadedBytes =
            await response.Content.ReadAsByteArrayAsync();

        downloadedBytes
            .Should()
            .Equal(png.Content);
    }

    [Fact]
    public async Task UnrelatedUser_CannotDownloadAttachment()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var pdf =
            MailboxTestHelper.CreatePdfAttachment();

        var createdMessage =
            await MailboxTestHelper.SendAndReadMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id,
                attachments: [pdf]);

        var attachment =
            createdMessage.Attachments.Single();

        var response =
            await context.UnrelatedUserClient.GetAsync(
                $"/api/mailbox/messages/{createdMessage.Id}" +
                $"/attachments/{attachment.Id}/download");

        response.StatusCode
            .Should()
            .BeOneOf(
                HttpStatusCode.Forbidden,
                HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvalidFileExtension_ReturnsBadRequest()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var invalidAttachment =
            new MailboxTestAttachment
            {
                FileName =
                    $"malicious-{Guid.NewGuid():N}.exe",

                ContentType =
                    "application/octet-stream",

                Content =
                [
                    0x4D,
                    0x5A,
                    0x90,
                    0x00
                ]
            };

        var response =
            await MailboxTestHelper.SendMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id,
                "Invalid extension test",
                "Geçersiz dosya uzantısı testi.",
                [invalidAttachment]);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvalidPngSignature_ReturnsBadRequest()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var invalidPng =
            new MailboxTestAttachment
            {
                FileName =
                    $"fake-{Guid.NewGuid():N}.png",

                ContentType =
                    "image/png",

                /*
                 * PNG uzantısı ve MIME türü doğru görünmesine rağmen
                 * içerik imzası geçersizdir.
                 */
                Content =
                [
                    0x54,
                    0x48,
                    0x49,
                    0x53,
                    0x49,
                    0x53,
                    0x4E,
                    0x4F,
                    0x54,
                    0x50,
                    0x4E,
                    0x47
                ]
            };

        var response =
            await MailboxTestHelper.SendMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id,
                "Invalid PNG signature",
                "Dosya imzası testi.",
                [invalidPng]);

        var responseText =
            await response.Content.ReadAsStringAsync();

        response.StatusCode
            .Should()
            .Be(
                HttpStatusCode.BadRequest,
                $"geçersiz PNG reddedilmeliydi. " +
                $"Response: {responseText}");
    }

    [Fact]
    public async Task RecipientDeletesMessage_MessageRemainsForSender()
    {
        using var context =
            await MailboxTestHelper.CreateContextAsync(
                _factory);

        var createdMessage =
            await MailboxTestHelper.SendAndReadMessageAsync(
                context.SenderClient,
                context.RecipientAuthentication.User!.Id);

        var deleteResponse =
            await context.RecipientClient.DeleteAsync(
                $"/api/mailbox/messages/{createdMessage.Id}");

        deleteResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var recipientDetailResponse =
            await context.RecipientClient.GetAsync(
                $"/api/mailbox/messages/{createdMessage.Id}");

        recipientDetailResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);

        /*
         * Alıcının silmesi, gönderenin gönderilen kutusundaki
         * mesajı kaldırmamalıdır.
         */
        var senderDetailResponse =
            await context.SenderClient.GetAsync(
                $"/api/mailbox/messages/{createdMessage.Id}");

        senderDetailResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MailboxEndpoints_WithoutAuthentication_ReturnUnauthorized()
    {
        using var anonymousClient =
            _factory.CreateClient();

        var response =
            await anonymousClient.GetAsync(
                "/api/mailbox/inbox?page=1&pageSize=20");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}