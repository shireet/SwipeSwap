using MediatR;

namespace SwipeSwap.Application.Exchanges.Dtos;

public record AcceptExchangeRequest(int ExchangeId, int ActorUserId) : IRequest<ExchangeDto>;