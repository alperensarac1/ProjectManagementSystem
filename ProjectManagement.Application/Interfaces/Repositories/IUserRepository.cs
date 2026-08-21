using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Interfaces.Repositories;

public interface IUserRepository
{
 
    Task<User?> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

  
    Task<User?> GetByIdForUpdateAsync(
        int userId,
        CancellationToken cancellationToken = default);


    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);


    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);


    Task<bool> ExistsByEmailExceptUserAsync(
        string email,
        int excludedUserId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyCollection<User> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        UserRole? role,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default);

    void Update(User user);

    void Remove(User user);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}