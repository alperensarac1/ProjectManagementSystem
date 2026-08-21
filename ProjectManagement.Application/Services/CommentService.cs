using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.DTOs.Comments;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.Mappings;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Services;

public sealed class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IProjectTaskRepository _taskRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IUserRepository _userRepository;

    public CommentService(
        ICommentRepository commentRepository,
        IProjectTaskRepository taskRepository,
        IProjectMemberRepository projectMemberRepository,
        IUserRepository userRepository)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
        _projectMemberRepository = projectMemberRepository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyCollection<CommentResponseDto>>
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

        var comments =
            await _commentRepository.GetByTaskIdAsync(
                taskId,
                cancellationToken);

        return comments
            .Select(comment =>
                comment.ToResponseDto(
                    currentUserId,
                    currentUserRole,
                    task.Project.OwnerId))
            .ToArray();
    }


    public async Task<CommentResponseDto> CreateAsync(
        int taskId,
        CreateCommentRequestDto request,
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

        await EnsureCanWriteCommentAsync(
            task,
            currentUserId,
            currentUserRole,
            cancellationToken);

        var user =
            await _userRepository.GetByIdAsync(
                currentUserId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "Yorumu oluşturacak kullanıcı bulunamadı.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException(
                "Pasif kullanıcı yorum oluşturamaz.");
        }

        var comment = new Comment
        {
            TaskId = task.Id,
            UserId = currentUserId,
            Content = request.Content.Trim()
        };

        await _commentRepository.AddAsync(
            comment,
            cancellationToken);

        await _commentRepository.SaveChangesAsync(
            cancellationToken);

        comment.User = user;
        comment.Task = task;

        return comment.ToResponseDto(
            currentUserId,
            currentUserRole,
            task.Project.OwnerId);
    }

    public async Task<CommentResponseDto> UpdateAsync(
        int taskId,
        int commentId,
        UpdateCommentRequestDto request,
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

        await EnsureCanWriteCommentAsync(
            task,
            currentUserId,
            currentUserRole,
            cancellationToken);

        var comment =
            await _commentRepository.GetByIdForUpdateAsync(
                commentId,
                cancellationToken);

        if (comment is null ||
            comment.TaskId != taskId)
        {
            throw new NotFoundException(
                "Güncellenecek yorum bulunamadı.");
        }

        if (comment.UserId != currentUserId)
        {
            throw new ForbiddenException(
                "Yalnızca kendi yorumunuzu güncelleyebilirsiniz.");
        }

        comment.Content =
            request.Content.Trim();

        _commentRepository.Update(
            comment);

        await _commentRepository.SaveChangesAsync(
            cancellationToken);

        if (comment.User is null)
        {
            comment.User =
                await _userRepository.GetByIdAsync(
                    comment.UserId,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Yorumu oluşturan kullanıcı bulunamadı.");
        }

        return comment.ToResponseDto(
            currentUserId,
            currentUserRole,
            task.Project.OwnerId);
    }

    public async Task DeleteAsync(
        int taskId,
        int commentId,
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

        /*
         * Silme yetkisi yalnızca yazma yetkisi bulunan kullanıcılara
         * açık olmalıdır. Viewer kullanıcı burada engellenir.
         */
        await EnsureCanWriteCommentAsync(
            task,
            currentUserId,
            currentUserRole,
            cancellationToken);

        var comment =
            await _commentRepository.GetByIdForUpdateAsync(
                commentId,
                cancellationToken);

        if (comment is null ||
            comment.TaskId != taskId)
        {
            throw new NotFoundException(
                "Silinecek yorum bulunamadı.");
        }

        var canDelete =
            currentUserRole == UserRole.Admin ||
            task.Project.OwnerId == currentUserId ||
            comment.UserId == currentUserId;

        if (!canDelete)
        {
            throw new ForbiddenException(
                "Bu yorumu silme yetkiniz bulunmamaktadır.");
        }

        _commentRepository.Remove(
            comment);

        await _commentRepository.SaveChangesAsync(
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
                "Bu görevin yorumlarını görüntüleme yetkiniz bulunmamaktadır.");
        }
    }

    private async Task EnsureCanWriteCommentAsync(
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
                "Bu görevde yorum işlemi yapma yetkiniz bulunmamaktadır.");
        }

        if (membership.Role ==
            ProjectMemberRole.Viewer)
        {
            throw new ForbiddenException(
                "Viewer rolündeki kullanıcılar yorum ekleyemez veya değiştiremez.");
        }
    }

    private static void EnsureProjectIsWritable(
        Project project)
    {
        if (project.IsArchived)
        {
            throw new BusinessRuleException(
                "Arşivlenmiş projelerde yorum değişikliği yapılamaz.");
        }
    }
}