using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Auth.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}