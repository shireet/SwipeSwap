using MediatR;

namespace SwipeSwap.Application.Profile.Dtos;

public record GetCurrentUserRequest(int UserId) : IRequest<CurrentUserDto>;
