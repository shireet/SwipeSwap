using MediatR;
using SwipeSwap.Application.Auth.Dtos;
using SwipeSwap.Domain.Exceptions;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Jwt.Auth.Interfaces;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Application.Auth.Register;

public class RegisterRequestHandler(
    IUserRepository userRepository,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository) : IRequestHandler<RegisterUserRequest, AuthResult>
{
    private const int RefreshTokenExpireDays = 14;

    public async Task<AuthResult> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
            throw new EmailAlreadyInUseException(request.Email);

        var user = new User()
        {
            Email = request.Email,
            EncryptedSensitiveData = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName
        };

        await userRepository.UpsertAsync(user, cancellationToken);
        
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