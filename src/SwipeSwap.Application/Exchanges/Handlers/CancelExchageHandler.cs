using MediatR;
using SwipeSwap.Application.Exchanges.Dtos;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Application.Exchanges.Handlers;

public class CancelExchangeHandler(IExchangeRepository repo) : IRequestHandler<CancelExchangeRequest, ExchangeDto>
{
    public async Task<ExchangeDto> Handle(CancelExchangeRequest cmd, CancellationToken ct)
    {
        var ex = await repo.GetByIdAsync(cmd.ExchangeId, ct) 
                 ?? throw new KeyNotFoundException("Exchange not found");
        ex.Cancel(cmd.ActorUserId, cmd.Reason);
        await repo.SaveChangesAsync(ct);
        return ExchangeDto.FromEntity(ex);
    }
}