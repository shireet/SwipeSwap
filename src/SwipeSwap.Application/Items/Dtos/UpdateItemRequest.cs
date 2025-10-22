using MediatR;
using SwipeSwap.Domain.Models.Enums;

namespace SwipeSwap.Application.Items.Dtos;

public sealed record UpdateItemRequest(
    int id,
    int OwnerId,
    string? Title = null,
    string? Description = null,
    bool? IsActive = null,
    List<string>? Tags = null,
    ItemCondition? Condition = null,
    string? City = null
) : IRequest<bool>;

