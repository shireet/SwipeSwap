namespace SwipeSwap.Application.Exchanges.Dtos;

public record ExchangeDto(
    int Id,
    int InitiatorId,
    int ReceiverId,
    int OfferedItemId,
    int RequestedItemId,
    string Status,
    DateTime CreatedAt
);