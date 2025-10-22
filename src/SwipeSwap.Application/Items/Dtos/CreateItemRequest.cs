using SwipeSwap.Domain.Models.Enums;

namespace  SwipeSwap.Application.Items.Dtos;

public record CreateItemRequest(
    int OwnerId,
    string Title,
    string? Description,
    string? ImageUrl,
    List<string>? Tags,
    ItemCondition? Condition,  
    string? City                
) : MediatR.IRequest<int>;