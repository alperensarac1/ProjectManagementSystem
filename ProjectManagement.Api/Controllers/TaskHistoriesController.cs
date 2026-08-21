using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.TaskHistories;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;


[ApiController]
[Authorize]
[EnableRateLimiting("general")]
[Route("api/tasks/{taskId:int}/histories")]
public sealed class TaskHistoriesController : ControllerBase
{
    private readonly ITaskHistoryService _taskHistoryService;
    private readonly ICurrentUserService _currentUserService;

    public TaskHistoriesController(
        ITaskHistoryService taskHistoryService,
        ICurrentUserService currentUserService)
    {
        _taskHistoryService = taskHistoryService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyCollection<TaskHistoryResponseDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistories(
        int taskId,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _taskHistoryService.GetByTaskIdAsync(
                taskId,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<IReadOnlyCollection<TaskHistoryResponseDto>>
                .Succeed(
                    result,
                    "Görev geçmişi başarıyla getirildi."));
    }
}