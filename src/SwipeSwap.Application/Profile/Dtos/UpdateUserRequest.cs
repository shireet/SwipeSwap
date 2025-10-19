using MediatR;

namespace SwipeSwap.Application.Profile.Dtos;

public record UpdateUserRequest : IRequest<CurrentUserDto>
{
    public required string DisplayName { get; init; }
}
