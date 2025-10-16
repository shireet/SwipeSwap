using Microsoft.EntityFrameworkCore;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Postgres.Context;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Infrastructure.Postgres.Repositories.Implementations;

public class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetAsync(string token, CancellationToken cancellationTokentoken)
    {
        return await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task RevokeAsync(RefreshToken token, CancellationToken cancellationTokentoken)
    {
        token.Revoked = true;
        dbContext.Update(token);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationTokentoken)
    {
        dbContext.RefreshTokens.Add(token);
        await dbContext.SaveChangesAsync();
    }
}
