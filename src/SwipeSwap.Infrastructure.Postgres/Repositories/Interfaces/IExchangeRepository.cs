using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Repositories.Interfaces;

public interface IExchangeRepository
{
    Task<bool> ExistsOpenForPairAsync(int initiatorId, int offeredItemId, int requestedItemId, CancellationToken ct);
    Task AddAsync(Exchange exchange, CancellationToken ct);
    Task<Exchange?> GetByIdAsync(int id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct); 
}