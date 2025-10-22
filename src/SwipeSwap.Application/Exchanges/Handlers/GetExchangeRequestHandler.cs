using MediatR;
using SwipeSwap.Application.Exchanges.Dtos;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Application.Exchanges.Handlers;

public sealed class GetExchangeRequestHandler(IExchangeRepository repo)
    : IRequestHandler<GetExchangeRequest, ExchangeDto?>
{
    public async Task<ExchangeDto?> Handle(GetExchangeRequest request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null) return null;

        return ExchangeDto.FromEntity(entity);
    }
}