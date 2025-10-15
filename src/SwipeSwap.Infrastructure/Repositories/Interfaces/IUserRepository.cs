using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserAsync(int id);
    
    Task<User?> GetByEmailAsync(string email);
    
    Task UpsertAsync(User user);
}