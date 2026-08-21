using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Interfaces.Repositories;


public interface ITaskHistoryRepository
{

    Task<IReadOnlyCollection<TaskHistory>> GetByTaskIdAsync(
        int taskId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TaskHistory history,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<TaskHistory> histories,
        CancellationToken cancellationToken = default);
}