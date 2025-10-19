// SwipeSwap.Application/Exchanges/CreateExchangeCommand.cs
using MediatR;
using SwipeSwap.Application.Exchanges.Dtos;

namespace SwipeSwap.Application.Exchanges;

public record CreateExchangeCommand(
    int InitiatorUserId,
    int OfferedItemId,
    int RequestedItemId,
    string? Message
) : IRequest<ExchangeDto>;