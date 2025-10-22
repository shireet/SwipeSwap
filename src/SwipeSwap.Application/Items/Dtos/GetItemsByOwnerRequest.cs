using MediatR;

namespace SwipeSwap.Application.Items.Dtos;

public sealed record GetItemsByOwnerRequest(int OwnerId) : IRequest<List<ItemDto>>;
