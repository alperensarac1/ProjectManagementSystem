using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Interfaces.Repositories;


public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashForUpdateAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RefreshToken>>
        GetActiveTokensByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default);

    Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);

    void Update(
        RefreshToken refreshToken);

    Task RevokeAllActiveTokensAsync(
        int userId,
        DateTime revokedAtUtc,
        string? revokedByIp,
        string reason,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}