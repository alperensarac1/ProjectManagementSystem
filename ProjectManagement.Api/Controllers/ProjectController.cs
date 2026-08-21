using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjectManagement.Application.Common.Extensions;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Projects;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("general")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ICurrentUserService _currentUserService;

    private readonly IValidator<CreateProjectRequestDto>
        _createValidator;

    private readonly IValidator<UpdateProjectRequestDto>
        _updateValidator;

    private readonly IValidator<ProjectListQueryDto>
        _listQueryValidator;

    public ProjectsController(
        IProjectService projectService,
        ICurrentUserService currentUserService,
        IValidator<CreateProjectRequestDto> createValidator,
        IValidator<UpdateProjectRequestDto> updateValidator,
        IValidator<ProjectListQueryDto> listQueryValidator)
    {
        _projectService = projectService;
        _currentUserService = currentUserService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _listQueryValidator = listQueryValidator;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<ProjectResponseDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ProjectListQueryDto query,
        CancellationToken cancellationToken)
    {
        await _listQueryValidator.ValidateAndThrowAppAsync(
            query,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _projectService.GetPagedAsync(
                query,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<PagedResult<ProjectResponseDto>>.Succeed(
                result,
                "Projeler başarıyla getirildi."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<ProjectResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _projectService.GetByIdAsync(
                id,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<ProjectResponseDto>.Succeed(
                result,
                "Proje bilgileri başarıyla getirildi."));
    }

    [Authorize(Roles = "Admin,ProjectManager")]
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<ProjectResponseDto>),
        StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectRequestDto request,
        CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _projectService.CreateAsync(
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<ProjectResponseDto>.Succeed(
                result,
                "Proje başarıyla oluşturuldu."));
    }

    [Authorize(Roles = "Admin,ProjectManager")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<ProjectResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProjectRequestDto request,
        CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _projectService.UpdateAsync(
                id,
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<ProjectResponseDto>.Succeed(
                result,
                "Proje başarıyla güncellendi."));
    }

    [Authorize(Roles = "Admin,ProjectManager")]
    [HttpPatch("{id:int}/archive")]
    [ProducesResponseType(
        typeof(ApiResponse<ProjectResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateArchiveStatus(
        int id,
        [FromBody] UpdateProjectArchiveRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _projectService.UpdateArchiveStatusAsync(
                id,
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        var message =
            request.IsArchived
                ? "Proje başarıyla arşivlendi."
                : "Proje arşivden çıkarıldı.";

        return Ok(
            ApiResponse<ProjectResponseDto>.Succeed(
                result,
                message));
    }


    [Authorize(Roles = "Admin,ProjectManager")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        await _projectService.DeleteAsync(
            id,
            currentUser.UserId,
            currentUser.Role,
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                new { },
                "Proje başarıyla silindi."));
    }
}