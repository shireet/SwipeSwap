using SwipeSwap.Domain.Models;
using SwipeSwap.Domain.Models.Enums;
using SwipeSwap.Domain.Shared;

namespace SwipeSwap.Infrastructure.Repositories.Interfaces;

public interface IItemRepository
{
    Task<PagedResult<Item>> GetCatalogAsync(
        int page, int pageSize,
        string? sortBy, string? sortDir,
        int? categoryId, ItemCondition? condition,
        string? city, string? search,
        string[]? tags, bool onlyActive,
        CancellationToken ct = default);
     Task<List<Item>> GetByOwnerAsync(int ownerId, CancellationToken ct = default);
    Task<int> AddTagToItemAsync(int itemId, string tagName);
    Task<int> RemoveTagFromItemAsync(int itemId, string tagName);
    Task<int> UpsertAsync(Item item);
    Task<Item?> GetByIdAsync(int id, CancellationToken ct = default);
}