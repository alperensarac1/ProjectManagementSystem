using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.DTOs.Tasks;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Mappings;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Services;

public sealed class ProjectTaskService
    : IProjectTaskService
{
    private readonly IProjectTaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITaskHistoryRepository _historyRepository;

    public ProjectTaskService(
        IProjectTaskRepository taskRepository,
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IUserRepository userRepository,
        ITaskHistoryRepository historyRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _userRepository = userRepository;
        _historyRepository = historyRepository;
    }

    public async Task<PagedResult<TaskResponseDto>> GetPagedAsync(
        TaskListQueryDto query,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var result = await _taskRepository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Search,
            query.ProjectId,
            query.AssignedToUserId,
            query.Status,
            query.Priority,
            query.IsOverdue,
            currentUserId,
            currentUserRole,
            cancellationToken);

        var tasks = result.Items
            .Select(task => task.ToResponseDto())
            .ToArray();

        return PagedResult<TaskResponseDto>.Create(
            tasks,
            query.Page,
            query.PageSize,
            result.TotalCount);
    }

    public async Task<TaskResponseDto> GetByIdAsync(
        int taskId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(
            taskId,
            cancellationToken);

        await EnsureCanViewTaskAsync(
            task,
            currentUserId,
            currentUserRole,
            cancellationToken);

        return task.ToResponseDto();
    }

    public async Task<TaskResponseDto> CreateAsync(
        CreateTaskRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var project =
            await _projectRepository.GetByIdAsync(
                request.ProjectId,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(
                "Görevin ekleneceği proje bulunamadı.");
        }

        EnsureProjectIsWritable(project);

        await EnsureCanCreateOrManageTaskAsync(
            project,
            currentUserId,
            currentUserRole,
            cancellationToken);

        ValidateDueDate(
            request.DueDate,
            project.StartDate);

        User? assignedUser = null;

        if (request.AssignedToUserId.HasValue)
        {
            assignedUser =
                await ValidateAssignedUserAsync(
                    project.Id,
                    request.AssignedToUserId.Value,
                    cancellationToken);
        }

        var task = new ProjectTask
        {
            ProjectId = project.Id,

            Title = request.Title.Trim(),

            Description =
                NormalizeOptionalText(request.Description),

            AssignedToUserId =
                assignedUser?.Id,

            CreatedByUserId =
                currentUserId,

            Status =
                request.Status,

            Priority =
                request.Priority,

            DueDate =
                request.DueDate,

            EstimatedHours =
                request.EstimatedHours,

            CompletedAt =
                request.Status == ProjectTaskStatus.Done
                    ? DateTime.UtcNow
                    : null
        };

        await _taskRepository.AddAsync(
            task,
            cancellationToken);

        await _taskRepository.SaveChangesAsync(
            cancellationToken);

        task.CreatedByUser =
            await _userRepository.GetByIdAsync(
                currentUserId,
                cancellationToken)
            ?? throw new NotFoundException(
                "Görevi oluşturan kullanıcı bulunamadı.");

        return task.ToResponseDto();
    }

public async Task<TaskResponseDto> UpdateAsync(
    int taskId,
    UpdateTaskRequestDto request,
    int currentUserId,
    UserRole currentUserRole,
    CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(request);

    var task =
        await GetTaskForUpdateOrThrowAsync(
            taskId,
            cancellationToken);

    EnsureProjectIsWritable(
        task.Project);

    await EnsureCanCreateOrManageTaskAsync(
        task.Project,
        currentUserId,
        currentUserRole,
        cancellationToken);

    ValidateDueDate(
        request.DueDate,
        task.Project.StartDate);

    User? assignedUser = null;

    if (request.AssignedToUserId.HasValue)
    {
        assignedUser =
            await ValidateAssignedUserAsync(
                task.ProjectId,
                request.AssignedToUserId.Value,
                cancellationToken);
    }

    var histories =
        new List<TaskHistory>();

    if (task.Status != request.Status)
    {
        histories.Add(
            CreateHistory(
                task.Id,
                currentUserId,
                TaskChangeType.StatusChanged,
                task.Status.ToString(),
                request.Status.ToString(),
                "Görev durumu güncellendi."));
    }

    if (task.Priority != request.Priority)
    {
        histories.Add(
            CreateHistory(
                task.Id,
                currentUserId,
                TaskChangeType.PriorityChanged,
                task.Priority.ToString(),
                request.Priority.ToString(),
                "Görev önceliği güncellendi."));
    }

    if (task.AssignedToUserId !=
        request.AssignedToUserId)
    {
        histories.Add(
            CreateHistory(
                task.Id,
                currentUserId,
                TaskChangeType.AssignedUserChanged,
                task.AssignedToUserId?.ToString(),
                request.AssignedToUserId?.ToString(),
                "Görevin atandığı kullanıcı değiştirildi."));
    }

    var normalizedDescription =
        NormalizeOptionalText(
            request.Description);

    var generalFieldsChanged =
        task.Title != request.Title.Trim() ||
        task.Description != normalizedDescription ||
        task.DueDate != request.DueDate ||
        task.EstimatedHours != request.EstimatedHours;

    if (generalFieldsChanged)
    {
        histories.Add(
            CreateHistory(
                task.Id,
                currentUserId,
                TaskChangeType.Updated,
                null,
                null,
                "Görevin genel bilgileri güncellendi."));
    }

    task.Title =
        request.Title.Trim();

    task.Description =
        normalizedDescription;

   
    task.AssignedToUserId =
        assignedUser?.Id;

    task.Status =
        request.Status;

    task.Priority =
        request.Priority;

    task.DueDate =
        request.DueDate;

    task.EstimatedHours =
        request.EstimatedHours;

    ApplyCompletedAtRule(
        task);

    if (histories.Count > 0)
    {
        await _historyRepository.AddRangeAsync(
            histories,
            cancellationToken);
    }

  
    await _taskRepository.SaveChangesAsync(
        cancellationToken);

    task.AssignedToUser =
        assignedUser;

    return task.ToResponseDto();
    
}
    public async Task<TaskResponseDto> UpdateStatusAsync(
        int taskId,
        UpdateTaskStatusRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var task =
            await GetTaskForUpdateOrThrowAsync(
                taskId,
                cancellationToken);

        EnsureProjectIsWritable(task.Project);

        await EnsureCanChangeStatusAsync(
            task,
            currentUserId,
            currentUserRole,
            cancellationToken);

        if (task.Status == request.Status)
        {
            return task.ToResponseDto();
        }

        var oldStatus = task.Status;

        task.Status = request.Status;

        ApplyCompletedAtRule(task);

        await _historyRepository.AddAsync(
            CreateHistory(
                task.Id,
                currentUserId,
                TaskChangeType.StatusChanged,
                oldStatus.ToString(),
                request.Status.ToString(),
                "Görev durumu değiştirildi."),
            cancellationToken);

        _taskRepository.Update(task);

        await _taskRepository.SaveChangesAsync(
            cancellationToken);

        return task.ToResponseDto();
    }

    public async Task<TaskResponseDto> AssignAsync(
        int taskId,
        AssignTaskRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var task =
            await GetTaskForUpdateOrThrowAsync(
                taskId,
                cancellationToken);

        EnsureProjectIsWritable(task.Project);

        await EnsureCanCreateOrManageTaskAsync(
            task.Project,
            currentUserId,
            currentUserRole,
            cancellationToken);

        User? assignedUser = null;

        if (request.AssignedToUserId.HasValue)
        {
            assignedUser =
                await ValidateAssignedUserAsync(
                    task.ProjectId,
                    request.AssignedToUserId.Value,
                    cancellationToken);
        }

        if (task.AssignedToUserId ==
            request.AssignedToUserId)
        {
            return task.ToResponseDto();
        }

        var oldAssignedUserId =
            task.AssignedToUserId;

        task.AssignedToUserId =
            assignedUser?.Id;

        task.AssignedToUser =
            assignedUser;

        await _historyRepository.AddAsync(
            CreateHistory(
                task.Id,
                currentUserId,
                TaskChangeType.AssignedUserChanged,
                oldAssignedUserId?.ToString(),
                assignedUser?.Id.ToString(),
                assignedUser is null
                    ? "Görev kullanıcı atamasından çıkarıldı."
                    : "Görev başka bir kullanıcıya atandı."),
            cancellationToken);

        _taskRepository.Update(task);

        await _taskRepository.SaveChangesAsync(
            cancellationToken);

        return task.ToResponseDto();
    }

    public async Task DeleteAsync(
        int taskId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        var task =
            await GetTaskForUpdateOrThrowAsync(
                taskId,
                cancellationToken);

        EnsureProjectIsWritable(task.Project);

        var canDelete =
            currentUserRole == UserRole.Admin ||
            (
                currentUserRole == UserRole.ProjectManager &&
                task.Project.OwnerId == currentUserId
            );

        if (!canDelete)
        {
            throw new ForbiddenException(
            "Bu görevi silme yetkiniz bulunmamaktadır.");
        }

        _taskRepository.Remove(task);

        await _taskRepository.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<ProjectTask> GetTaskOrThrowAsync(
        int taskId,
        CancellationToken cancellationToken)
    {
        var task =
            await _taskRepository.GetByIdAsync(
                taskId,
                cancellationToken);

        if (task is null)
        {
            throw new NotFoundException(
                "Görev bulunamadı.");
        }

        return task;
    }

    private async Task<ProjectTask> GetTaskForUpdateOrThrowAsync(
        int taskId,
        CancellationToken cancellationToken)
    {
        var task =
            await _taskRepository.GetByIdForUpdateAsync(
                taskId,
                cancellationToken);

        if (task is null)
        {
            throw new NotFoundException(
                "Görev bulunamadı.");
        }

        return task;
    }

    private async Task EnsureCanViewTaskAsync(
        ProjectTask task,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken)
    {
        if (currentUserRole == UserRole.Admin)
        {
            return;
        }

        if (task.Project.OwnerId == currentUserId)
        {
            return;
        }

        var membership =
            await _projectMemberRepository.GetAsync(
                task.ProjectId,
                currentUserId,
                cancellationToken);

        if (membership is null || !membership.IsActive)
        {
            throw new ForbiddenException(
             "Bu görevi görüntüleme yetkiniz bulunmamaktadır.");
        }
    }

    private async Task EnsureCanCreateOrManageTaskAsync(
        Project project,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken)
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

        var membership =
            await _projectMemberRepository.GetAsync(
                project.Id,
                currentUserId,
                cancellationToken);

        if (membership is not null &&
            membership.IsActive &&
            membership.Role ==
                ProjectMemberRole.Contributor)
        {
            return;
        }

        throw new ForbiddenException(
     "Bu görevin durumunu değiştirme yetkiniz bulunmamaktadır.");
    }

    private async Task EnsureCanChangeStatusAsync(
        ProjectTask task,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken)
    {
        if (currentUserRole == UserRole.Admin)
        {
            return;
        }

        if (currentUserRole == UserRole.ProjectManager &&
            task.Project.OwnerId == currentUserId)
        {
            return;
        }

        var membership =
            await _projectMemberRepository.GetAsync(
                task.ProjectId,
                currentUserId,
                cancellationToken);

        if (membership is null ||
            !membership.IsActive ||
            membership.Role == ProjectMemberRole.Viewer)
        {
           throw new ForbiddenException(
         "Bu görevin durumunu değiştirme yetkiniz bulunmamaktadır.");
        }

        if (membership.Role ==
            ProjectMemberRole.Contributor)
        {
            return;
        }
        if (membership.Role ==
                ProjectMemberRole.Member &&
            task.AssignedToUserId ==
                currentUserId)
        {
            return;
        }

        throw new ForbiddenException(
         "Yalnızca size atanmış görevleri değiştirebilirsiniz.");
    }

    private async Task<User> ValidateAssignedUserAsync(
        int projectId,
        int userId,
        CancellationToken cancellationToken)
    {
        var user =
            await _userRepository.GetByIdAsync(
                userId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "Görevin atanacağı kullanıcı bulunamadı.");
        }

        if (!user.IsActive)
        {
            throw new BusinessRuleException(
                "Pasif kullanıcıya görev atanamaz.");
        }

        var membership =
            await _projectMemberRepository.GetAsync(
                projectId,
                userId,
                cancellationToken);

        if (membership is null || !membership.IsActive)
        {
            throw new BusinessRuleException(
                "Görev yalnızca projenin aktif bir üyesine atanabilir.");
        }

        if (membership.Role ==
            ProjectMemberRole.Viewer)
        {
            throw new BusinessRuleException(
                "Viewer rolündeki kullanıcıya görev atanamaz.");
        }

        return user;
    }

    private static void EnsureProjectIsWritable(
        Project project)
    {
        if (project.IsArchived)
        {
            throw new BusinessRuleException(
                "Arşivlenmiş projelerde görev değişikliği yapılamaz.");
        }
    }

    private static void ValidateDueDate(
        DateTime? dueDate,
        DateTime projectStartDate)
    {
        if (dueDate.HasValue &&
            dueDate.Value < projectStartDate)
        {
            throw new BusinessRuleException(
                "Görev teslim tarihi proje başlangıç tarihinden önce olamaz.");
        }
    }

    private static void ApplyCompletedAtRule(
        ProjectTask task)
    {
        if (task.Status ==
            ProjectTaskStatus.Done)
        {
            task.CompletedAt ??=
                DateTime.UtcNow;
        }
        else
        {
            task.CompletedAt = null;
        }
    }

    private static TaskHistory CreateHistory(
        int taskId,
        int changedByUserId,
        TaskChangeType changeType,
        string? oldValue,
        string? newValue,
        string description)
    {
        return new TaskHistory
        {
            TaskId = taskId,
            ChangedByUserId = changedByUserId,
            ChangeType = changeType,
            OldValue = oldValue,
            NewValue = newValue,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}