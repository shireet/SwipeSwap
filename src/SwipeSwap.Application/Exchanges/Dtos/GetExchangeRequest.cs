using MediatR;
using MediatR;
using SwipeSwap.Application.Exchanges.Dtos;

namespace SwipeSwap.Application.Exchanges.Handlers;

public sealed record GetExchangeRequest(int Id) : IRequest<ExchangeDto?>;
