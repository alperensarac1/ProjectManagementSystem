using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjectManagement.Application.Common.Extensions;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.TaskTimeLogs;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;


[ApiController]
[Authorize]
[EnableRateLimiting("general")]
[Route("api/tasks/{taskId:int}/time-logs")]
public sealed class TaskTimeLogsController : ControllerBase
{
    private readonly ITaskTimeLogService _timeLogService;
    private readonly ICurrentUserService _currentUserService;

    private readonly IValidator<CreateTaskTimeLogRequestDto>
        _createValidator;

    private readonly IValidator<UpdateTaskTimeLogRequestDto>
        _updateValidator;

    public TaskTimeLogsController(
        ITaskTimeLogService timeLogService,
        ICurrentUserService currentUserService,
        IValidator<CreateTaskTimeLogRequestDto> createValidator,
        IValidator<UpdateTaskTimeLogRequestDto> updateValidator)
    {
        _timeLogService = timeLogService;
        _currentUserService = currentUserService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

  
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyCollection<TaskTimeLogResponseDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeLogs(
        int taskId,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _timeLogService.GetByTaskIdAsync(
                taskId,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<IReadOnlyCollection<TaskTimeLogResponseDto>>
                .Succeed(
                    result,
                    "Zaman kayıtları başarıyla getirildi."));
    }


    [HttpGet("summary")]
    [ProducesResponseType(
        typeof(ApiResponse<TaskTimeLogSummaryDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        int taskId,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _timeLogService.GetSummaryAsync(
                taskId,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<TaskTimeLogSummaryDto>.Succeed(
                result,
                "Zaman kaydı özeti başarıyla getirildi."));
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<TaskTimeLogResponseDto>),
        StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        int taskId,
        [FromBody] CreateTaskTimeLogRequestDto request,
        CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _timeLogService.CreateAsync(
                taskId,
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<TaskTimeLogResponseDto>.Succeed(
                result,
                "Zaman kaydı başarıyla eklendi."));
    }

    [HttpPut("{timeLogId:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<TaskTimeLogResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        int taskId,
        int timeLogId,
        [FromBody] UpdateTaskTimeLogRequestDto request,
        CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _timeLogService.UpdateAsync(
                taskId,
                timeLogId,
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<TaskTimeLogResponseDto>.Succeed(
                result,
                "Zaman kaydı başarıyla güncellendi."));
    }

    [HttpDelete("{timeLogId:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        int taskId,
        int timeLogId,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        await _timeLogService.DeleteAsync(
            taskId,
            timeLogId,
            currentUser.UserId,
            currentUser.Role,
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                new { },
                "Zaman kaydı başarıyla silindi."));
    }
}