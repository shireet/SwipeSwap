using MediatR;
using SwipeSwap.Application.Exchanges.Dtos;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Application.Exchanges.Handlers;

public class DeclineExchangeHandler(IExchangeRepository repo) : IRequestHandler<DeclineExchangeRequest, ExchangeDto>
{
    public async Task<ExchangeDto> Handle(DeclineExchangeRequest cmd, CancellationToken ct)
    {
        var ex = await repo.GetByIdAsync(cmd.ExchangeId, ct) 
                 ?? throw new KeyNotFoundException("Exchange not found");
        ex.Decline(cmd.ActorUserId, cmd.Reason);
        await repo.SaveChangesAsync(ct);
        return ExchangeDto.FromEntity(ex);
    }
}