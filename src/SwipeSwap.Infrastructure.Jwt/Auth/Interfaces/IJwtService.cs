using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Jwt.Auth.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}