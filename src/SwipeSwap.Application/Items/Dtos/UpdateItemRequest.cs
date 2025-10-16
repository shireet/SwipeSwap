using MediatR;

namespace SwipeSwap.Application.Items;

public record UpdateItemRequest(int id, int OwnerId, string? Title, string? Description, bool? IsActive, List<string>? Tags): IRequest<bool>;