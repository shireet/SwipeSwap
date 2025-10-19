using MediatR;
using SwipeSwap.Application.Profile.Dtos;
using SwipeSwap.Domain.Exceptions;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Application.Profile;

public class GetUserByIdHandler(
    IUserRepository userRepository
    ): IRequestHandler<GetUserByIdRequest, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserAsync(request.Id, cancellationToken);
        return user == null ? throw new UserNotFoundException(request.Id) : new UserDto(user.Id, user.Email, user.DisplayName);
    }
}