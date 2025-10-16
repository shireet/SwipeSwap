using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserAsync(int id, CancellationToken cancellationToken);
    
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    
    Task UpsertAsync(User user, CancellationToken cancellationToken);
}