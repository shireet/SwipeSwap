
using MediatR;

namespace  SwipeSwap.Application.Items;

public record CreateItemRequest(int OwnerId, string Title, string? Description, List<string> Tags) : IRequest<int>;