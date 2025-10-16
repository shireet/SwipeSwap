using MediatR;

namespace SwipeSwap.Application.Auth.Dtos;

public record RegisterUserRequest(string Email, string Password, string DisplayName) : IRequest<AuthResult>;
