using MediatR;

namespace SwipeSwap.Application.Exchanges.Dtos;

public record CreateExchangeRequest(
    int InitiatorUserId,
    int OfferedItemId,
    int RequestedItemId,
    string? Message
) : IRequest<ExchangeDto>;