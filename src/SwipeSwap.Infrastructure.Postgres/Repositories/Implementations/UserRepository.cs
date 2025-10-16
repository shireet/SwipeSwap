using Microsoft.EntityFrameworkCore;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Postgres.Context;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Infrastructure.Postgres.Repositories.Implementations;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetUserAsync(int id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken: cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken: cancellationToken);
    }

    public async Task UpsertAsync(User user, CancellationToken cancellationToken)
    {
        var existingUser = await dbContext.Users.FindAsync(user.Id, cancellationToken);
        
        if (existingUser != null)
        {
            dbContext.Entry(existingUser).CurrentValues.SetValues(user);
        }
        else
        {
            await dbContext.Users.AddAsync(user, cancellationToken);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}