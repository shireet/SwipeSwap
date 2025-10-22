using MediatR;

namespace SwipeSwap.Application.Exchanges.Dtos;

public sealed record GetExchangeRequest(int Id) : IRequest<ExchangeDto?>;
