using MediatR;

namespace SwipeSwap.Application.Exchanges.Dtos;

public record DeclineExchangeRequest(int ExchangeId, int ActorUserId, string? Reason) : IRequest<ExchangeDto>;