using MediatR;

namespace SwipeSwap.Application.Dtos.Items;

public sealed record GetItemsByOwnerRequest(int OwnerId) : IRequest<List<ItemDto>>;
