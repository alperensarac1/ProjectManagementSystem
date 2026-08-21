using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Projects;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Mappings;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Services;

public sealed class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public ProjectService(
        IProjectRepository projectRepository,
        IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<ProjectResponseDto>> GetPagedAsync(
        ProjectListQueryDto query,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var result =
            await _projectRepository.GetPagedAsync(
                query.Page,
                query.PageSize,
                query.Search,
                query.Status,
                query.IsArchived,
                query.OwnerId,
                currentUserId,
                currentUserRole,
                cancellationToken);

        var projects =
            result.Items
                .Select(project =>
                    project.ToResponseDto())
                .ToArray();

        return PagedResult<ProjectResponseDto>.Create(
            projects,
            query.Page,
            query.PageSize,
            result.TotalCount);
    }

    public async Task<ProjectResponseDto> GetByIdAsync(
        int projectId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        var project =
            await _projectRepository.GetByIdAsync(
                projectId,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(
                "Proje bulunamadı.");
        }

        var canView =
            await CanViewProjectAsync(
                project,
                currentUserId,
                currentUserRole,
                cancellationToken);

        if (!canView)
        {
            throw new ForbiddenException(
                "Bu projeyi görüntüleme yetkiniz bulunmamaktadır.");
        }

        return project.ToResponseDto();
    }

    public async Task<ProjectResponseDto> CreateAsync(
        CreateProjectRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUserRole is not UserRole.Admin and
            not UserRole.ProjectManager)
        {
            throw new ForbiddenException(
                "Proje oluşturma yetkiniz bulunmamaktadır.");
        }

        if (request.EndDate.HasValue &&
            request.EndDate.Value < request.StartDate)
        {
            throw new BusinessRuleException(
                "Proje bitiş tarihi başlangıç tarihinden önce olamaz.");
        }

        var nameExists =
            await _projectRepository.ExistsByNameAsync(
                request.Name,
                cancellationToken);

        if (nameExists)
        {
            throw new ConflictException(
                "Aynı isimde aktif bir proje bulunmaktadır.");
        }

        int ownerId;

        if (currentUserRole == UserRole.Admin)
        {
            ownerId =
                request.OwnerId ??
                currentUserId;
        }
        else
        {
            ownerId = currentUserId;
        }

        var owner =
            await ValidateProjectOwnerAsync(
                ownerId,
                cancellationToken);

        var project = new Project
        {
            Name = request.Name.Trim(),

            Description =
                NormalizeOptionalText(
                    request.Description),

            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status,
            OwnerId = owner.Id,
            IsArchived = false,
            ArchivedAt = null
        };

        project.Members.Add(
            new ProjectMember
            {
                UserId = owner.Id,
                Role =
                    ProjectMemberRole.Contributor,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            });

        await _projectRepository.AddAsync(
            project,
            cancellationToken);

        await _projectRepository.SaveChangesAsync(
            cancellationToken);

        project.Owner = owner;

        return project.ToResponseDto();
    }

    public async Task<ProjectResponseDto> UpdateAsync(
        int projectId,
        UpdateProjectRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var project =
            await _projectRepository.GetByIdForUpdateAsync(
                projectId,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(
                "Güncellenecek proje bulunamadı.");
        }

        EnsureCanManageProject(
            project,
            currentUserId,
            currentUserRole);

        if (request.EndDate.HasValue &&
            request.EndDate.Value < request.StartDate)
        {
            throw new BusinessRuleException(
                "Proje bitiş tarihi başlangıç tarihinden önce olamaz.");
        }

        var nameUsedByAnotherProject =
            await _projectRepository
                .ExistsByNameExceptProjectAsync(
                    request.Name,
                    projectId,
                    cancellationToken);

        if (nameUsedByAnotherProject)
        {
            throw new ConflictException(
                "Aynı isimde başka bir aktif proje bulunmaktadır.");
        }

        if (currentUserRole == UserRole.Admin &&
            request.OwnerId.HasValue &&
            request.OwnerId.Value != project.OwnerId)
        {
            var newOwner =
                await ValidateProjectOwnerAsync(
                    request.OwnerId.Value,
                    cancellationToken);

            project.OwnerId = newOwner.Id;
            project.Owner = newOwner;

            var existingOwnerMembership =
                project.Members.FirstOrDefault(
                    member =>
                        member.UserId ==
                        newOwner.Id);

            if (existingOwnerMembership is null)
            {
                project.Members.Add(
                    new ProjectMember
                    {
                        UserId = newOwner.Id,

                        Role =
                            ProjectMemberRole
                                .Contributor,

                        JoinedAt = DateTime.UtcNow,
                        IsActive = true
                    });
            }
            else
            {
                existingOwnerMembership.IsActive =
                    true;

                existingOwnerMembership.Role =
                    ProjectMemberRole.Contributor;
            }
        }

        project.Name =
            request.Name.Trim();

        project.Description =
            NormalizeOptionalText(
                request.Description);

        project.StartDate =
            request.StartDate;

        project.EndDate =
            request.EndDate;

        project.Status =
            request.Status;

        _projectRepository.Update(project);

        await _projectRepository.SaveChangesAsync(
            cancellationToken);

        return project.ToResponseDto();
    }

    public async Task<ProjectResponseDto>
        UpdateArchiveStatusAsync(
            int projectId,
            UpdateProjectArchiveRequestDto request,
            int currentUserId,
            UserRole currentUserRole,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var project =
            await _projectRepository.GetByIdForUpdateAsync(
                projectId,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(
                "Proje bulunamadı.");
        }

        EnsureCanManageProject(
            project,
            currentUserId,
            currentUserRole);

        project.IsArchived =
            request.IsArchived;

        project.ArchivedAt =
            request.IsArchived
                ? DateTime.UtcNow
                : null;

        _projectRepository.Update(project);

        await _projectRepository.SaveChangesAsync(
            cancellationToken);

        return project.ToResponseDto();
    }

    public async Task DeleteAsync(
        int projectId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        var project =
            await _projectRepository.GetByIdForUpdateAsync(
                projectId,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(
                "Silinecek proje bulunamadı.");
        }

        EnsureCanManageProject(
            project,
            currentUserId,
            currentUserRole);

        _projectRepository.Remove(project);

        await _projectRepository.SaveChangesAsync(
            cancellationToken);
    }


    private async Task<User> ValidateProjectOwnerAsync(
        int ownerId,
        CancellationToken cancellationToken)
    {
        var owner =
            await _userRepository.GetByIdAsync(
                ownerId,
                cancellationToken);

        if (owner is null)
        {
            throw new NotFoundException(
                "Proje sahibi olarak seçilen kullanıcı bulunamadı.");
        }

        if (!owner.IsActive)
        {
            throw new BusinessRuleException(
                "Pasif kullanıcı proje sahibi yapılamaz.");
        }

        if (owner.Role is not UserRole.Admin and
            not UserRole.ProjectManager)
        {
            throw new BusinessRuleException(
                "Proje sahibi yalnızca Admin veya ProjectManager olabilir.");
        }

        return owner;
    }
    private async Task<bool> CanViewProjectAsync(
        Project project,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken)
    {
        if (currentUserRole == UserRole.Admin)
        {
            return true;
        }

        if (project.OwnerId == currentUserId)
        {
            return true;
        }

        return await _projectRepository
            .IsUserActiveMemberAsync(
                project.Id,
                currentUserId,
                cancellationToken);
    }

    /// <summary>
    /// Kullanıcının projeyi yönetme yetkisini doğrular.
    /// </summary>
    private static void EnsureCanManageProject(
        Project project,
        int currentUserId,
        UserRole currentUserRole)
    {
        if (currentUserRole == UserRole.Admin)
        {
            return;
        }

        if (currentUserRole == UserRole.ProjectManager &&
            project.OwnerId == currentUserId)
        {
            return;
        }

        throw new ForbiddenException(
            "Bu projeyi yönetme yetkiniz bulunmamaktadır.");
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}