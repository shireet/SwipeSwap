using MediatR;

namespace SwipeSwap.Application.Auth.Dtos;

public record RefreshTokenRequest(string RefreshToken) : IRequest<AuthResult>;