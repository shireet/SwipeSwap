using MediatR;

namespace SwipeSwap.Application.Auth;

public record RegisterUserRequest(string Email, string Password, string DisplayName) : IRequest<string>;
