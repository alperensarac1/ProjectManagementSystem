using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.DTOs.ProjectMembers;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Mappings;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Services;


public sealed class ProjectMemberService
    : IProjectMemberService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IUserRepository _userRepository;

    public ProjectMemberService(
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyCollection<ProjectMemberResponseDto>>
        GetMembersAsync(
            int projectId,
            int currentUserId,
            UserRole currentUserRole,
            bool includeInactive,
            CancellationToken cancellationToken = default)
    {
        var project =
            await GetProjectOrThrowAsync(
                projectId,
                cancellationToken);

        var canView =
            await CanViewProjectAsync(
                project,
                currentUserId,
                currentUserRole,
                cancellationToken);

        if (!canView)
        {
   
            throw new ForbiddenException(
                "Bu projenin ekip üyelerini görüntüleme yetkiniz bulunmamaktadır.");
        }


        var canViewInactiveMembers =
            currentUserRole == UserRole.Admin ||
            project.OwnerId == currentUserId;

        var shouldIncludeInactive =
            includeInactive &&
            canViewInactiveMembers;

        var members =
            await _projectMemberRepository.GetByProjectIdAsync(
                projectId,
                shouldIncludeInactive,
                cancellationToken);

        return members
            .Select(member =>
                member.ToResponseDto(project.OwnerId))
            .ToArray();
    }


    public async Task<ProjectMemberResponseDto> AddMemberAsync(
        int projectId,
        AddProjectMemberRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var project =
            await GetProjectOrThrowAsync(
                projectId,
                cancellationToken);

        EnsureCanManageMembers(
            project,
            currentUserId,
            currentUserRole);

        if (project.IsArchived)
        {
            throw new BusinessRuleException(
                "Arşivlenmiş projelere yeni üye eklenemez.");
        }

        var user =
            await _userRepository.GetByIdAsync(
                request.UserId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "Projeye eklenecek kullanıcı bulunamadı.");
        }

        if (!user.IsActive)
        {
            throw new BusinessRuleException(
                "Pasif kullanıcı projeye eklenemez.");
        }


        if (user.Id == project.OwnerId)
        {
            throw new ConflictException(
                "Proje sahibi zaten proje ekibinin bir üyesidir.");
        }

 
        var existingMembership =
            await _projectMemberRepository
                .GetByProjectAndUserForUpdateAsync(
                    projectId,
                    user.Id,
                    cancellationToken);

        if (existingMembership is not null)
        {
            if (existingMembership.IsActive)
            {
                throw new ConflictException(
                    "Kullanıcı zaten bu projenin aktif bir üyesidir.");
            }
            existingMembership.IsActive = true;
            existingMembership.Role = request.Role;
            existingMembership.JoinedAt = DateTime.UtcNow;

            _projectMemberRepository.Update(
                existingMembership);

            await _projectMemberRepository.SaveChangesAsync(
                cancellationToken);

            existingMembership.User = user;

            return existingMembership.ToResponseDto(
                project.OwnerId);
        }

        var projectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = user.Id,
            Role = request.Role,
            JoinedAt = DateTime.UtcNow,
            IsActive = true

        };

        await _projectMemberRepository.AddAsync(
            projectMember,
            cancellationToken);

        await _projectMemberRepository.SaveChangesAsync(
            cancellationToken);

  
        projectMember.User = user;

        return projectMember.ToResponseDto(
            project.OwnerId);
    }

    public async Task<ProjectMemberResponseDto> UpdateMemberAsync(
        int projectId,
        int userId,
        UpdateProjectMemberRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var project =
            await GetProjectOrThrowAsync(
                projectId,
                cancellationToken);

        EnsureCanManageMembers(
            project,
            currentUserId,
            currentUserRole);

        if (project.IsArchived)
        {
            throw new BusinessRuleException(
                "Arşivlenmiş projelerin üyelik rolleri değiştirilemez.");
        }

        if (userId == project.OwnerId)
        {
            throw new BusinessRuleException(
                "Proje sahibinin proje içi rolü değiştirilemez.");
        }

        var membership =
            await _projectMemberRepository
                .GetByProjectAndUserForUpdateAsync(
                    projectId,
                    userId,
                    cancellationToken);

        if (membership is null ||
            !membership.IsActive)
        {
            throw new NotFoundException(
                "Aktif proje üyeliği bulunamadı.");
        }

        membership.Role = request.Role;

        _projectMemberRepository.Update(
            membership);

        await _projectMemberRepository.SaveChangesAsync(
            cancellationToken);

        return membership.ToResponseDto(
            project.OwnerId);
    }

 
    public async Task RemoveMemberAsync(
        int projectId,
        int userId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        var project =
            await GetProjectOrThrowAsync(
                projectId,
                cancellationToken);

        EnsureCanManageMembers(
            project,
            currentUserId,
            currentUserRole);

        if (project.IsArchived)
        {
            throw new BusinessRuleException(
                "Arşivlenmiş projelerin üyeleri değiştirilemez.");
        }

        if (userId == project.OwnerId)
        {
            throw new BusinessRuleException(
                "Proje sahibi ekip üyeliğinden çıkarılamaz.");
        }

    
        var membership =
            await _projectMemberRepository
                .GetByProjectAndUserForUpdateAsync(
                    projectId,
                    userId,
                    cancellationToken);

        if (membership is null ||
            !membership.IsActive)
        {
            throw new NotFoundException(
                "Çıkarılacak aktif proje üyeliği bulunamadı.");
        }

        membership.IsActive = false;

        _projectMemberRepository.Update(
            membership);

        await _projectMemberRepository.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<Project> GetProjectOrThrowAsync(
        int projectId,
        CancellationToken cancellationToken)
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

        return project;
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

        return await _projectMemberRepository
            .IsActiveMemberAsync(
                project.Id,
                currentUserId,
                cancellationToken);
    }

    private static void EnsureCanManageMembers(
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
            "Bu projenin ekip üyelerini yönetme yetkiniz bulunmamaktadır.");
    }
}