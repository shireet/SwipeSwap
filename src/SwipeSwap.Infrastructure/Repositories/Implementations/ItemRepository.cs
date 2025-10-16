using Microsoft.EntityFrameworkCore;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Context;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Infrastructure.Repositories.Implementations;

public class ItemRepository(AppDbContext dbContext) : IItemRepository
{
    public async Task<int> UpsertAsync(Item item)
    {
        var existingItem = await dbContext.Items.FindAsync(item.Id);

        if (existingItem is not null)
        {
            dbContext.Entry(existingItem).CurrentValues.SetValues(item);
        }
        else
        {
            dbContext.Items.Add(item);
        }

        await dbContext.SaveChangesAsync();
        return item.Id;
    }

    public async Task<Item?> GetByIdAsync(int id, bool asNoTracking = true)
    {
        IQueryable<Item> query = dbContext.Items
            .Include(i => i.ItemTags)
            .ThenInclude(it => it.Tag);

        if (asNoTracking)
            query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(i => i.Id == id);
    }
}