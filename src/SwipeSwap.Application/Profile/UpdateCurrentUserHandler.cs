using MediatR;
using SwipeSwap.Application.Profile.Dtos;
using SwipeSwap.Domain.Exceptions;
using SwipeSwap.Infrastructure.Jwt.Services.Interfaces;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Application.Profile;

public class UpdateCurrentUserHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService) : IRequestHandler<UpdateUserRequest, CurrentUserDto>
{
    public async Task<CurrentUserDto> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var user = await userRepository.GetUserAsync(userId, cancellationToken);
        if (user is null)
            throw new UserNotFoundException(userId);

        user.DisplayName = request.DisplayName;
        await userRepository.UpsertAsync(user, cancellationToken);

        return new CurrentUserDto(user.Id, user.DisplayName, user.Email);
    }
}