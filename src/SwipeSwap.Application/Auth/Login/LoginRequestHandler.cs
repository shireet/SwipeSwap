using MediatR;
using SwipeSwap.Application.Auth.Dtos;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Jwt.Auth.Interfaces;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Application.Auth.Login;

public class LoginRequestHandler(
    IUserRepository userRepository,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository) : IRequestHandler<LoginUserRequest, AuthResult>
{
    private const int RefreshTokenExpireDays = 14;
    public async Task<AuthResult> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
                   ?? throw new UnauthorizedAccessException("Invalid credentials.");
    
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.EncryptedSensitiveData))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }
        
        var access = jwtService.GenerateToken(user);
        var refresh = new RefreshToken
        {
            UserId = user.Id,
            Token =  Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpireDays)
        };

        await refreshTokenRepository.AddAsync(refresh, cancellationToken);

        return new AuthResult(access, refresh.Token);
    }
}