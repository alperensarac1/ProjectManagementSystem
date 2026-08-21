using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjectManagement.Application.Common.Extensions;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.ProjectMembers;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;


[ApiController]
[Authorize]
[EnableRateLimiting("general")]
[Route("api/projects/{projectId:int}/members")]
public sealed class ProjectMembersController : ControllerBase
{
    private readonly IProjectMemberService _projectMemberService;
    private readonly ICurrentUserService _currentUserService;

    private readonly IValidator<AddProjectMemberRequestDto>
        _addMemberValidator;

    private readonly IValidator<UpdateProjectMemberRequestDto>
        _updateMemberValidator;

    public ProjectMembersController(
        IProjectMemberService projectMemberService,
        ICurrentUserService currentUserService,
        IValidator<AddProjectMemberRequestDto> addMemberValidator,
        IValidator<UpdateProjectMemberRequestDto> updateMemberValidator)
    {
        _projectMemberService = projectMemberService;
        _currentUserService = currentUserService;
        _addMemberValidator = addMemberValidator;
        _updateMemberValidator = updateMemberValidator;
    }


    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<
            IReadOnlyCollection<ProjectMemberResponseDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers(
        int projectId,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _projectMemberService.GetMembersAsync(
                projectId,
                currentUser.UserId,
                currentUser.Role,
                includeInactive,
                cancellationToken);

        return Ok(
            ApiResponse<
                IReadOnlyCollection<ProjectMemberResponseDto>>
                .Succeed(
                    result,
                    "Proje üyeleri başarıyla getirildi."));
    }


    [Authorize(Roles = "Admin,ProjectManager")]
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<ProjectMemberResponseDto>),
        StatusCodes.Status201Created)]
    public async Task<IActionResult> AddMember(
        int projectId,
        [FromBody] AddProjectMemberRequestDto request,
        CancellationToken cancellationToken)
    {
        await _addMemberValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _projectMemberService.AddMemberAsync(
                projectId,
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<ProjectMemberResponseDto>.Succeed(
                result,
                "Kullanıcı projeye başarıyla eklendi."));
    }

    [Authorize(Roles = "Admin,ProjectManager")]
    [HttpPut("{userId:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<ProjectMemberResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMember(
        int projectId,
        int userId,
        [FromBody] UpdateProjectMemberRequestDto request,
        CancellationToken cancellationToken)
    {
        await _updateMemberValidator.ValidateAndThrowAppAsync(
            request,
            cancellationToken);

        var currentUser =
            _currentUserService.GetCurrentUser();

        var result =
            await _projectMemberService.UpdateMemberAsync(
                projectId,
                userId,
                request,
                currentUser.UserId,
                currentUser.Role,
                cancellationToken);

        return Ok(
            ApiResponse<ProjectMemberResponseDto>.Succeed(
                result,
                "Proje üyesinin rolü başarıyla güncellendi."));
    }

    [Authorize(Roles = "Admin,ProjectManager")]
    [HttpDelete("{userId:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveMember(
        int projectId,
        int userId,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserService.GetCurrentUser();

        await _projectMemberService.RemoveMemberAsync(
            projectId,
            userId,
            currentUser.UserId,
            currentUser.Role,
            cancellationToken);

        return Ok(
            ApiResponse<object>.Succeed(
                new { },
                "Kullanıcı proje ekibinden başarıyla çıkarıldı."));
    }
}