using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.DTOs.TaskHistories;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Mappings;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Services;


public sealed class TaskHistoryService
    : ITaskHistoryService
{
    private readonly IProjectTaskRepository _taskRepository;
    private readonly ITaskHistoryRepository _historyRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;

    public TaskHistoryService(
        IProjectTaskRepository taskRepository,
        ITaskHistoryRepository historyRepository,
        IProjectMemberRepository projectMemberRepository)
    {
        _taskRepository = taskRepository;
        _historyRepository = historyRepository;
        _projectMemberRepository = projectMemberRepository;
    }

    public async Task<IReadOnlyCollection<TaskHistoryResponseDto>>
        GetByTaskIdAsync(
            int taskId,
            int currentUserId,
            UserRole currentUserRole,
            CancellationToken cancellationToken = default)
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

        await EnsureCanViewTaskAsync(
            task,
            currentUserId,
            currentUserRole,
            cancellationToken);

        var histories =
            await _historyRepository.GetByTaskIdAsync(
                taskId,
                cancellationToken);

        return histories
            .Select(history =>
                history.ToResponseDto())
            .ToArray();
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
                "Bu görevin geçmişini görüntüleme yetkiniz bulunmamaktadır.");
        }
    }
}