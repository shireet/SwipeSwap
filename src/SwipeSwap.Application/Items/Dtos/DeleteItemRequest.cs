using MediatR;

namespace SwipeSwap.Application.Items;

public record DeleteItemRequest(int ItemId, int OwnerId) : IRequest<bool>;