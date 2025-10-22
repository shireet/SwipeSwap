using Microsoft.EntityFrameworkCore;
using SwipeSwap.Domain.Models;
using SwipeSwap.Domain.Models.Enums;            
using SwipeSwap.Infrastructure.Postgres.Context;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Infrastructure.Postgres.Repositories.Implementations;

public sealed class ExchangeRepository : IExchangeRepository
{
    private readonly AppDbContext _db;
    public ExchangeRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsOpenForPairAsync(
        int initiatorId,
        int offeredItemId,
        int requestedItemId,
        CancellationToken ct)
    {
        return _db.Set<Exchange>()
            .AsNoTracking()
            .AnyAsync(x =>
                x.InitiatorId    == initiatorId &&
                x.OfferedItemId  == offeredItemId &&
                x.RequestedItemId== requestedItemId &&
                (x.Status == ExchangeStatus.Sent || x.Status == ExchangeStatus.Accepted),
                ct);
    }

    public Task AddAsync(Exchange exchange, CancellationToken ct) =>
        _db.Set<Exchange>().AddAsync(exchange, ct).AsTask();

    public Task<Exchange?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Set<Exchange>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}