using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjectManagement.Api.Models.Mailbox;
using ProjectManagement.Application.Common.Extensions;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Mailbox;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("general")]
public sealed class MailboxController : ControllerBase
{
 
    private const long MaximumRequestBodySize =
        210L * 1024L * 1024L;

    private readonly IMailboxService _mailboxService;
    private readonly ICurrentUserService _currentUserService;

    private readonly IValidator<SendMailboxMessageDto>
        _sendValidator;

    private readonly IValidator<MailboxListQueryDto>
        _listQueryValidator;

    public MailboxController(
        IMailboxService mailboxService,
        ICurrentUserService currentUserService,
        IValidator<SendMailboxMessageDto> sendValidator,
        IValidator<MailboxListQueryDto> listQueryValidator)
    {
        _mailboxService = mailboxService;
        _currentUserService = currentUserService;
        _sendValidator = sendValidator;
        _listQueryValidator = listQueryValidator;
    }
    
    [HttpPost("messages")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaximumRequestBodySize)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = MaximumRequestBodySize)]
    [ProducesResponseType(
        typeof(ApiResponse<MailboxMessageDetailDto>),
        StatusCodes.Status201Created)]
    public async Task<IActionResult> Send(
        [FromForm] SendMailboxMessageRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();
        
        var openedStreams =
            new List<Stream>();

        try
        {
            var uploadedFiles =
                new List<UploadedMailboxFileDto>();

            foreach (var attachment in request.Attachments)
            {
                var stream =
                    attachment.OpenReadStream();

                openedStreams.Add(stream);

                uploadedFiles.Add(
                    new UploadedMailboxFileDto
                    {
                        FileName =
                            attachment.FileName,

                        ContentType =
                            attachment.ContentType,

                        Length =
                            attachment.Length,

                        Content =
                            stream
                    });
            }

            var applicationRequest =
                new SendMailboxMessageDto
                {
                    RecipientUserIds =
                        request.RecipientUserIds,

                    Subject =
                        request.Subject,

                    Body =
                        request.Body,

                    Attachments =
                        uploadedFiles
                };

            await _sendValidator.ValidateAndThrowAppAsync(
                applicationRequest,
                cancellationToken);

            var result =
                await _mailboxService.SendAsync(
                    currentUser.UserId,
                    applicationRequest,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    messageId = result.Id
                },
                ApiResponse<MailboxMessageDetailDto>.Succeed(
                    result,
                    "Mesaj başarıyla gönderildi."));
        }
        finally
        {
            foreach (var stream in openedStreams)
            {
                await stream.DisposeAsync();
            }
        }
    }
    
    [HttpGet("inbox")]
    [ProducesResponseType(
        typeof(
            ApiResponse<
                PagedResult<MailboxMessageListItemDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInbox(
        [FromQuery] MailboxListQueryDto query,
        CancellationToken cancellationToken)
    {
        await _listQueryValidator.ValidateAndThrowAppAsync(
            query,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _mailboxService.GetInboxAsync(
                currentUser.UserId,
                query,
                cancellationToken);

        return Ok(
            ApiResponse<
                PagedResult<MailboxMessageListItemDto>>
                .Succeed(
                    result,
                    "Gelen kutusu başarıyla getirildi."));
    }
    
    [HttpGet("sent")]
    [ProducesResponseType(
        typeof(
            ApiResponse<
                PagedResult<MailboxMessageListItemDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSent(
        [FromQuery] MailboxListQueryDto query,
        CancellationToken cancellationToken)
    {
        await _listQueryValidator.ValidateAndThrowAppAsync(
            query,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _mailboxService.GetSentAsync(
                currentUser.UserId,
                query,
                cancellationToken);

        return Ok(
            ApiResponse<
                PagedResult<MailboxMessageListItemDto>>
                .Succeed(
                    result,
                    "Gönderilen mesajlar başarıyla getirildi."));
    }
    
    [HttpGet("messages/{messageId:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<MailboxMessageDetailDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        int messageId,
        [FromQuery] bool markAsRead = true,
        CancellationToken cancellationToken = default)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _mailboxService.GetByIdAsync(
                messageId,
                currentUser.UserId,
                markAsRead,
                cancellationToken);

        return Ok(
            ApiResponse<MailboxMessageDetailDto>.Succeed(
                result,
                "Mesaj ayrıntıları başarıyla getirildi."));
    }


    [HttpPatch("messages/{messageId:int}/read")]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsRead(
        int messageId,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        await _mailboxService.MarkAsReadAsync(
            messageId,
            currentUser.UserId,
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                new { },
                "Mesaj okundu olarak işaretlendi."));
    }
    
    [HttpPatch("messages/{messageId:int}/unread")]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsUnread(
        int messageId,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        await _mailboxService.MarkAsUnreadAsync(
            messageId,
            currentUser.UserId,
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                new { },
                "Mesaj okunmadı olarak işaretlendi."));
    }
    
    [HttpDelete("messages/{messageId:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        int messageId,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        await _mailboxService.DeleteAsync(
            messageId,
            currentUser.UserId,
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                new { },
                "Mesaj kutunuzdan başarıyla kaldırıldı."));
    }
    
    [HttpGet(
        "messages/{messageId:int}/attachments/{attachmentId:int}/download")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadAttachment(
        int messageId,
        int attachmentId,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _mailboxService.DownloadAttachmentAsync(
                messageId,
                attachmentId,
                currentUser.UserId,
                cancellationToken);

        return File(
            result.Content,
            result.ContentType,
            result.FileName,
            enableRangeProcessing: true);
    }
}