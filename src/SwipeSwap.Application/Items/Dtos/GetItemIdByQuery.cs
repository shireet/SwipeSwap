using MediatR;

namespace SwipeSwap.Application.Items.Dtos;

public record GetItemByIdQuery(int Id) : IRequest<ItemDto>;


