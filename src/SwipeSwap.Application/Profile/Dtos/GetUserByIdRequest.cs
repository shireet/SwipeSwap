using MediatR;

namespace SwipeSwap.Application.Profile.Dtos;

public record GetUserByIdRequest(int Id) : IRequest<UserDto>;