using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<IReadOnlyCollection<Comment>> GetByTaskIdAsync(
        int taskId,
        CancellationToken cancellationToken = default);

    Task<Comment?> GetByIdAsync(
        int commentId,
        CancellationToken cancellationToken = default);
    Task<Comment?> GetByIdForUpdateAsync(
        int commentId,
        CancellationToken cancellationToken = default);
    Task AddAsync(
        Comment comment,
        CancellationToken cancellationToken = default);
    void Update(Comment comment);
    void Remove(Comment comment);
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}