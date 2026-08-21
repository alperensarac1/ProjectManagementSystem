using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.DTOs.TaskTimeLogs;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Mappings;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Services;

public sealed class TaskTimeLogService
    : ITaskTimeLogService
{
    private readonly ITaskTimeLogRepository _timeLogRepository;
    private readonly IProjectTaskRepository _taskRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IUserRepository _userRepository;

    public TaskTimeLogService(
        ITaskTimeLogRepository timeLogRepository,
        IProjectTaskRepository taskRepository,
        IProjectMemberRepository projectMemberRepository,
        IUserRepository userRepository)
    {
        _timeLogRepository = timeLogRepository;
        _taskRepository = taskRepository;
        _projectMemberRepository = projectMemberRepository;
        _userRepository = userRepository;
    }


    public async Task<IReadOnlyCollection<TaskTimeLogResponseDto>>
        GetByTaskIdAsync(
            int taskId,
            int currentUserId,
            UserRole currentUserRole,
            CancellationToken cancellationToken = default)
    {
        var task =
            await GetTaskOrThrowAsync(
                taskId,
                cancellationToken);

        await EnsureCanViewTaskAsync(
            task,
            currentUserId,
            currentUserRole,
            cancellationToken);

        var timeLogs =
            await _timeLogRepository.GetByTaskIdAsync(
                taskId,
                cancellationToken);

        return timeLogs
            .Select(timeLog =>
                timeLog.ToResponseDto(
                    currentUserId,
                    currentUserRole,
                    task.Project.OwnerId))
            .ToArray();
    }


    public async Task<TaskTimeLogSummaryDto> GetSummaryAsync(
        int taskId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        var task =
            await GetTaskOrThrowAsync(
                taskId,
                cancellationToken);

        await EnsureCanViewTaskAsync(
            task,
            currentUserId,
            currentUserRole,
            cancellationToken);

        var timeLogs =
            await _timeLogRepository.GetByTaskIdAsync(
                taskId,
                cancellationToken);

        return task.ToSummaryDto(timeLogs);
    }

    public async Task<TaskTimeLogResponseDto> CreateAsync(
        int taskId,
        CreateTaskTimeLogRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var task =
            await GetTaskOrThrowAsync(
                taskId,
                cancellationToken);

        EnsureProjectIsWritable(
            task.Project);

        await EnsureCanWriteTimeLogAsync(
            task,
            currentUserId,
            currentUserRole,
            cancellationToken);

        ValidateWorkDate(
            request.WorkDate);

        var user =
            await _userRepository.GetByIdAsync(
                currentUserId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "Zaman kaydını oluşturacak kullanıcı bulunamadı.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException(
                "Pasif kullanıcı zaman kaydı oluşturamaz.");
        }

        var timeLog = new TaskTimeLog
        {
            TaskId = task.Id,
            UserId = currentUserId,
            Hours = request.Hours,

            Description =
                NormalizeOptionalText(
                    request.Description),

            WorkDate = request.WorkDate,
            CreatedAt = DateTime.UtcNow
        };

        await _timeLogRepository.AddAsync(
            timeLog,
            cancellationToken);

        await _timeLogRepository.SaveChangesAsync(
            cancellationToken);

        timeLog.Task = task;
        timeLog.User = user;

        return timeLog.ToResponseDto(
            currentUserId,
            currentUserRole,
            task.Project.OwnerId);
    }


    public async Task<TaskTimeLogResponseDto> UpdateAsync(
        int taskId,
        int timeLogId,
        UpdateTaskTimeLogRequestDto request,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var task =
            await GetTaskOrThrowAsync(
                taskId,
                cancellationToken);

        EnsureProjectIsWritable(
            task.Project);

        await EnsureCanWriteTimeLogAsync(
            task,
            currentUserId,
            currentUserRole,
            cancellationToken);

        ValidateWorkDate(
            request.WorkDate);

        var timeLog =
            await _timeLogRepository.GetByIdForUpdateAsync(
                timeLogId,
                cancellationToken);

        if (timeLog is null ||
            timeLog.TaskId != taskId)
        {
            throw new NotFoundException(
                "Güncellenecek zaman kaydı bulunamadı.");
        }

        if (timeLog.UserId != currentUserId)
        {
            throw new ForbiddenException(
                "Yalnızca kendi zaman kaydınızı güncelleyebilirsiniz.");
        }

        timeLog.Hours =
            request.Hours;

        timeLog.Description =
            NormalizeOptionalText(
                request.Description);

        timeLog.WorkDate =
            request.WorkDate;

        _timeLogRepository.Update(
            timeLog);

        await _timeLogRepository.SaveChangesAsync(
            cancellationToken);

        return timeLog.ToResponseDto(
            currentUserId,
            currentUserRole,
            task.Project.OwnerId);
    }

    public async Task DeleteAsync(
        int taskId,
        int timeLogId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        var task =
            await GetTaskOrThrowAsync(
                taskId,
                cancellationToken);

        EnsureProjectIsWritable(
            task.Project);

        await EnsureCanWriteTimeLogAsync(
            task,
            currentUserId,
            currentUserRole,
            cancellationToken);

        var timeLog =
            await _timeLogRepository.GetByIdForUpdateAsync(
                timeLogId,
                cancellationToken);

        if (timeLog is null ||
            timeLog.TaskId != taskId)
        {
            throw new NotFoundException(
                "Silinecek zaman kaydı bulunamadı.");
        }

        var canDelete =
            currentUserRole == UserRole.Admin ||
            task.Project.OwnerId == currentUserId ||
            timeLog.UserId == currentUserId;

        if (!canDelete)
        {
            throw new ForbiddenException(
                "Bu zaman kaydını silme yetkiniz bulunmamaktadır.");
        }

        _timeLogRepository.Remove(
            timeLog);

        await _timeLogRepository.SaveChangesAsync(
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

        if (membership is null ||
            !membership.IsActive)
        {
            throw new ForbiddenException(
                "Bu görevin zaman kayıtlarını görüntüleme yetkiniz bulunmamaktadır.");
        }
    }


    private async Task EnsureCanWriteTimeLogAsync(
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

        if (membership is null ||
            !membership.IsActive)
        {
            throw new ForbiddenException(
                "Bu görevde zaman kaydı işlemi yapma yetkiniz bulunmamaktadır.");
        }

        if (membership.Role ==
            ProjectMemberRole.Viewer)
        {
            throw new ForbiddenException(
                "Viewer rolündeki kullanıcılar zaman kaydı oluşturamaz veya değiştiremez.");
        }
    }


    private static void EnsureProjectIsWritable(
        Project project)
    {
        if (project.IsArchived)
        {
            throw new BusinessRuleException(
                "Arşivlenmiş projelerde zaman kaydı değişikliği yapılamaz.");
        }
    }

    private static void ValidateWorkDate(
        DateTime workDate)
    {
        if (workDate > DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "Çalışma tarihi gelecekte olamaz.");
        }
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}