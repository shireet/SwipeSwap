using SwipeSwap.Domain.Models;

namespace SwipeSwap.Application.Exchanges.Dtos;

public record ExchangeDto(
    int Id,
    int InitiatorId,
    int ReceiverId,
    int OfferedItemId,
    int RequestedItemId,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public static ExchangeDto FromEntity(Exchange e) => new(
        e.Id,
        e.InitiatorId,
        e.ReceiverId,
        e.OfferedItemId,
        e.RequestedItemId,
        e.Status.ToString(),
        e.CreatedAt,
        e.UpdatedAt
    );
}