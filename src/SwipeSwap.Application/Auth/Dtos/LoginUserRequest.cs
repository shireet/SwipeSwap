using MediatR;

namespace SwipeSwap.Application.Auth.Dtos;

public record LoginUserRequest(string Email, string Password) : IRequest<AuthResult>;
