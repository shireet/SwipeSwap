using MediatR;
using SwipeSwap.Application.Exchanges.Dtos;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Application.Exchanges.Handlers;

public class AcceptExchangeHandler(IExchangeRepository repo) : IRequestHandler<AcceptExchangeRequest, ExchangeDto>
{
    public async Task<ExchangeDto> Handle(AcceptExchangeRequest cmd, CancellationToken ct)
    {
        var ex = await repo.GetByIdAsync(cmd.ExchangeId, ct) 
                 ?? throw new KeyNotFoundException("Exchange not found");
        ex.Accept(cmd.ActorUserId);
        await repo.SaveChangesAsync(ct);
        return ExchangeDto.FromEntity(ex);
    }
}