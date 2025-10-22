using MediatR;
using SwipeSwap.Application.Exchanges.Dtos;

namespace SwipeSwap.Application.Exchanges;

public record CreateExchangeRequest(
    int InitiatorUserId,
    int OfferedItemId,
    int RequestedItemId,
    string? Message
) : IRequest<ExchangeDto>;