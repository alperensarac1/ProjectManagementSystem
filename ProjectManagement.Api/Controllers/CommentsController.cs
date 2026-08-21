using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjectManagement.Application.Common.Extensions;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Comments;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;

[ApiController]
[Authorize]
[EnableRateLimiting("general")]
[Route("api/tasks/{taskId:int}/comments")]
public sealed class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ICurrentUserService _currentUserService;

    private readonly IValidator<CreateCommentRequestDto>
        _createValidator;

    private readonly IValidator<UpdateCommentRequestDto>
        _updateValidator;

    public CommentsController(
        ICommentService commentService,
        ICurrentUserService currentUserService,
        IValidator<CreateCommentRequestDto> createValidator,
        IValidator<UpdateCommentRequestDto> updateValidator)
    {
        _commentService = commentService;
        _currentUserService = currentUserService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

 
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyCollection<CommentResponseDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments(
        int taskId,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _commentService.GetByTaskIdAsync(
                taskId,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<IReadOnlyCollection<CommentResponseDto>>
                .Succeed(
                    result,
                    "Görev yorumları başarıyla getirildi."));
    }

   
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<CommentResponseDto>),
        StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        int taskId,
        [FromBody] CreateCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _commentService.CreateAsync(
                taskId,
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<CommentResponseDto>.Succeed(
                result,
                "Yorum başarıyla eklendi."));
    }


    [HttpPut("{commentId:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<CommentResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        int taskId,
        int commentId,
        [FromBody] UpdateCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _commentService.UpdateAsync(
                taskId,
                commentId,
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<CommentResponseDto>.Succeed(
                result,
                "Yorum başarıyla güncellendi."));
    }



    [HttpDelete("{commentId:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        int taskId,
        int commentId,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        await _commentService.DeleteAsync(
            taskId,
            commentId,
            currentUser.UserId,
            currentUser.Role,
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                new { },
                "Yorum başarıyla silindi."));
    }
}