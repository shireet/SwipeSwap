using MediatR;
using SwipeSwap.Application.Auth.Dtos;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Jwt.Auth.Interfaces;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Application.Auth.Refresh;

public class RefreshTokenHandler(
    IUserRepository userRepository,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository):  IRequestHandler<RefreshTokenRequest, AuthResult>
{
    private const int RefreshTokenExpireDays = 14;
    public async Task<AuthResult> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var token = await refreshTokenRepository.GetAsync(request.RefreshToken, cancellationToken)
                    ?? throw new UnauthorizedAccessException("Invalid refresh token.");
        if (token.Revoked || token.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired or revoked.");

        var user = await userRepository.GetUserAsync(token.UserId, cancellationToken) ?? throw new Exception("User not found.");
        
        await refreshTokenRepository.RevokeAsync(token, cancellationToken);
        var newAccess = jwtService.GenerateToken(user);
        var refresh = new RefreshToken
        {
            UserId = user.Id,
            Token =  Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpireDays)
        };
        await refreshTokenRepository.AddAsync(refresh, cancellationToken);
        return new AuthResult(newAccess, refresh.Token);
    }
}