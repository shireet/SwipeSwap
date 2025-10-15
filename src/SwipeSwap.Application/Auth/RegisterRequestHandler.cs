using MediatR;
using SwipeSwap.Domain.Exceptions;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Auth.Interfaces;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Application.Auth;

public class RegisterRequestHandler(
    IUserRepository userRepository,
    IJwtService jwtService) : IRequestHandler<RegisterUserRequest, string>
{
    public async Task<string> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new EmailAlreadyInUseException(request.Email);

        var user = new User()
        {
            Email = request.Email,
            EncryptedSensitiveData = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName
        };

        await userRepository.UpsertAsync(user);
        return jwtService.GenerateToken(user);
    }
}