namespace SwipeSwap.Application.Exchanges.Dtos;

public record CreateExchangeDto(
    int InitiatorUserId,
    int OfferedItemId,
    int RequestedItemId,
    string? Message
);