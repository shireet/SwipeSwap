using MediatR;
using SwipeSwap.Application.Profile.Dtos;
using SwipeSwap.Domain.Exceptions;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Application.Profile;

public class GetCurrentUserHandler(
    IUserRepository userRepository): IRequestHandler<GetCurrentUserRequest, CurrentUserDto>
{
    public async Task<CurrentUserDto> Handle(GetCurrentUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserAsync(request.UserId, cancellationToken);
        return user is null ? throw new UserNotFoundException(request.UserId) : new CurrentUserDto(user.Id, user.Email, user.DisplayName);
    }
}