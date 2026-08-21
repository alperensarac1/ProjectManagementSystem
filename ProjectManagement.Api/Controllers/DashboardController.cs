using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Dashboard;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ICurrentUserService _currentUserService;

    public DashboardController(
        IDashboardService dashboardService,
        ICurrentUserService currentUserService)
    {
        _dashboardService = dashboardService;
        _currentUserService = currentUserService;
    }

    [HttpGet("summary")]
    [ProducesResponseType(
        typeof(ApiResponse<DashboardSummaryDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _dashboardService.GetSummaryAsync(
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<DashboardSummaryDto>.Succeed(
                result,
                "Dashboard özeti başarıyla getirildi."));
    }

    [HttpGet("recent-tasks")]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyCollection<RecentTaskDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentTasks(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _dashboardService.GetRecentTasksAsync(
                currentUser.UserId,
                currentUser.Role,
                count,
                cancellationToken);

        return Ok(
            ApiResponse<IReadOnlyCollection<RecentTaskDto>>.Succeed(
                result,
                "Son görevler başarıyla getirildi."));
    }
}