using Microsoft.EntityFrameworkCore;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Context;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Infrastructure.Repositories.Implementations;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetUserAsync(int id)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task UpsertAsync(User user)
    {
        var existingUser = await dbContext.Users.FindAsync(user.Id);
        
        if (existingUser != null)
        {
            dbContext.Entry(existingUser).CurrentValues.SetValues(user);
        }
        else
        {
            await dbContext.Users.AddAsync(user);
        }
        await dbContext.SaveChangesAsync();
    }
}