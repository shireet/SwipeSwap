using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetAsync(string token, CancellationToken cancellationToken);
    Task RevokeAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
}