using MediatR;

namespace SwipeSwap.Application.Exchanges.Dtos;

public record CancelExchangeRequest(int ExchangeId, int ActorUserId, string? Reason) : IRequest<ExchangeDto>;