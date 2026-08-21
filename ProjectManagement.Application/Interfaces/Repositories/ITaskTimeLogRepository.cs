using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Interfaces.Repositories;

public interface ITaskTimeLogRepository
{
    Task<IReadOnlyCollection<TaskTimeLog>> GetByTaskIdAsync(
        int taskId,
        CancellationToken cancellationToken = default);
    Task<TaskTimeLog?> GetByIdAsync(
        int timeLogId,
        CancellationToken cancellationToken = default);
    Task<TaskTimeLog?> GetByIdForUpdateAsync(
        int timeLogId,
        CancellationToken cancellationToken = default);
    Task AddAsync(
        TaskTimeLog timeLog,
        CancellationToken cancellationToken = default);
    void Update(TaskTimeLog timeLog);
    void Remove(TaskTimeLog timeLog);
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}