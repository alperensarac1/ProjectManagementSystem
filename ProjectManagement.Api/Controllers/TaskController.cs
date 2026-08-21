using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.Common.Extensions;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Tasks;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class TasksController : ControllerBase
{
    private readonly IProjectTaskService _taskService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateTaskRequestDto> _createValidator;
    private readonly IValidator<UpdateTaskRequestDto> _updateValidator;
    private readonly IValidator<UpdateTaskStatusRequestDto> _statusValidator;
    private readonly IValidator<AssignTaskRequestDto> _assignValidator;
    private readonly IValidator<TaskListQueryDto> _listValidator;

    public TasksController(
        IProjectTaskService taskService,
        ICurrentUserService currentUserService,
        IValidator<CreateTaskRequestDto> createValidator,
        IValidator<UpdateTaskRequestDto> updateValidator,
        IValidator<UpdateTaskStatusRequestDto> statusValidator,
        IValidator<AssignTaskRequestDto> assignValidator,
        IValidator<TaskListQueryDto> listValidator)
    {
        _taskService = taskService;
        _currentUserService = currentUserService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _statusValidator = statusValidator;
        _assignValidator = assignValidator;
        _listValidator = listValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] TaskListQueryDto query,
        CancellationToken cancellationToken)
    {
        await _listValidator.ValidateAndThrowAppAsync(
            query,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _taskService.GetPagedAsync(
                query,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<PagedResult<TaskResponseDto>>.Succeed(
                result,
                "Görevler başarıyla getirildi."));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _taskService.GetByIdAsync(
                id,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<TaskResponseDto>.Succeed(
                result,
                "Görev bilgileri getirildi."));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTaskRequestDto request,
        CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _taskService.CreateAsync(
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<TaskResponseDto>.Succeed(
                result,
                "Görev başarıyla oluşturuldu."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateTaskRequestDto request,
        CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _taskService.UpdateAsync(
                id,
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<TaskResponseDto>.Succeed(
                result,
                "Görev başarıyla güncellendi."));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateTaskStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        await _statusValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _taskService.UpdateStatusAsync(
                id,
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<TaskResponseDto>.Succeed(
                result,
                "Görev durumu başarıyla değiştirildi."));
    }

    [HttpPatch("{id:int}/assign")]
    public async Task<IActionResult> Assign(
        int id,
        [FromBody] AssignTaskRequestDto request,
        CancellationToken cancellationToken)
    {
        await _assignValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _taskService.AssignAsync(
                id,
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<TaskResponseDto>.Succeed(
                result,
                "Görev ataması başarıyla değiştirildi."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        await _taskService.DeleteAsync(
            id,
            currentUser.UserId,
            currentUser.Role,
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                "Görev başarıyla silindi."));
    }
}