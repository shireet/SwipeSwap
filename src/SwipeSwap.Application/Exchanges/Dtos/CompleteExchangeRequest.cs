using MediatR;

namespace SwipeSwap.Application.Exchanges.Dtos;

public record CompleteExchangeRequest(int ExchangeId, int ActorUserId, string? Note) : IRequest<ExchangeDto>;