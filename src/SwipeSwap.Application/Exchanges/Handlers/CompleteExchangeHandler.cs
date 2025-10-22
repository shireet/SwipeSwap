using MediatR;
using SwipeSwap.Application.Exchanges.Dtos;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Application.Exchanges.Handlers;

public class CompleteExchangeHandler(IExchangeRepository repo) : IRequestHandler<CompleteExchangeRequest, ExchangeDto>
{
    public async Task<ExchangeDto> Handle(CompleteExchangeRequest cmd, CancellationToken ct)
    {
        var ex = await repo.GetByIdAsync(cmd.ExchangeId, ct) 
                 ?? throw new KeyNotFoundException("Exchange not found");
        ex.Complete(cmd.ActorUserId, cmd.Note);
        await repo.SaveChangesAsync(ct);
        return ExchangeDto.FromEntity(ex);
    }
}