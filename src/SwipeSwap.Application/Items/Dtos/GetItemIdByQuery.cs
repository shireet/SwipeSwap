using MediatR;

namespace SwipeSwap.Application.Items;

public record GetItemByIdQuery(int Id) : IRequest<ItemDto>;


