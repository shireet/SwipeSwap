using MediatR;

namespace SwipeSwap.Application.Items.Dtos;

public record DeleteItemRequest(int ItemId, int OwnerId) : IRequest<bool>;