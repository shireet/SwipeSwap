using Microsoft.EntityFrameworkCore;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Postgres.Context;
using SwipeSwap.Infrastructure.Postgres.Context;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Infrastructure.Repositories.Implementations;

public class ItemRepository(AppDbContext dbContext) : IItemRepository
{
    public async Task<int> AddTagToItemAsync(int itemId, string tagName)
    {
        var item = await dbContext.Items
            .Include(i => i.ItemTags)
            .ThenInclude(it => it.Tag)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item is null)
            throw new Exception($"Item with ID {itemId} not found.");

        var tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
        if (tag is null)
        {
            tag = new Tag { Name = tagName };
            dbContext.Tags.Add(tag);
            await dbContext.SaveChangesAsync();
        }

        if (item.ItemTags.Any(it => it.TagId == tag.Id))
            return item.Id; 

        item.ItemTags.Add(new ItemTag { ItemId = item.Id, TagId = tag.Id });
        await dbContext.SaveChangesAsync();
        return item.Id;
    }
    
    public async Task<int> RemoveTagFromItemAsync(int itemId, string tagName)
    {
        var item = await dbContext.Items
            .Include(i => i.ItemTags)
            .ThenInclude(it => it.Tag)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item is null)
            throw new Exception($"Item with ID {itemId} not found.");

        var itemTag = item.ItemTags.FirstOrDefault(it => it.Tag.Name == tagName);
        if (itemTag is null)
            return item.Id; 

        item.ItemTags.Remove(itemTag);
        await dbContext.SaveChangesAsync();
        return item.Id;
    }
    
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