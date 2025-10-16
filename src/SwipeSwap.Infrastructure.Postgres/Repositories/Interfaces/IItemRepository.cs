using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Repositories.Interfaces;

public interface IItemRepository
{
    Task<int> AddTagToItemAsync(int itemId, string tagName);
    Task<int> RemoveTagFromItemAsync(int itemId, string tagName);
    Task<int> UpsertAsync(Item item);
    Task<Item?> GetByIdAsync(int id,  bool asNoTracking = true);
}