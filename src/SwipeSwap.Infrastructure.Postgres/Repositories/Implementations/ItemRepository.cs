using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SwipeSwap.Domain.Models;
using SwipeSwap.Domain.Models.Enums;
using SwipeSwap.Domain.Shared;
using SwipeSwap.Infrastructure.Postgres.Context;
using SwipeSwap.Infrastructure.Postgres.Context;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Infrastructure.Repositories.Implementations;

public class ItemRepository(AppDbContext dbContext) : IItemRepository
{
    public async Task<PagedResult<Item>> GetCatalogAsync(
    int page, int pageSize,
    string? sortBy, string? sortDir,
    int? categoryId, ItemCondition? condition,
    string? city, string? search,
    string[]? tags, bool onlyActive,
    CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        IQueryable<Item> q = dbContext.Items.AsNoTracking()
            .Include(i => i.ItemTags).ThenInclude(it => it.Tag);

        if (onlyActive) q = q.Where(i => i.IsActive);
        if (categoryId is not null) q = q.Where(i => i.CategoryId == categoryId);
        if (condition is not null) q = q.Where(i => i.Condition == condition);
        if (!string.IsNullOrWhiteSpace(city)) q = q.Where(i => i.City != null && i.City == city);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(i => EF.Functions.ILike(i.Title, $"%{s}%")
                           || (i.Description != null && EF.Functions.ILike(i.Description, $"%{s}%")));
        }
        if (tags is { Length: > 0 })
        {
            var normTags = tags.Where(t => !string.IsNullOrWhiteSpace(t))
                               .Select(t => t.Trim())
                               .Distinct(StringComparer.OrdinalIgnoreCase)
                               .ToArray();

            // Требуем, чтобы у предмета были все указанные теги (AND).
            foreach (var t in normTags)
                q = q.Where(i => i.ItemTags.Any(it => it.Tag!.Name == t));
        }

        // сортировка
        var sortMap = new Dictionary<string, Expression<Func<Item, object?>>>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["created"] = i => i.Id,      
            ["title"] = i => i.Title,
            ["city"] = i => i.City
        };

        var key = string.IsNullOrWhiteSpace(sortBy) ? "created" : sortBy!;
        if (!sortMap.TryGetValue(key, out var keySelector))
            keySelector = sortMap["created"];

        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        q = desc ? q.OrderByDescending(keySelector) : q.OrderBy(keySelector);

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<Item>(items, total, page, pageSize);
    }
    
    public async Task<List<Item>> GetByOwnerAsync(int ownerId, CancellationToken ct = default, bool asNoTracking = true)
    {
        IQueryable<Item> q = dbContext.Items
            .Include(i => i.ItemTags)
            .ThenInclude(it => it.Tag)
            .Where(i => i.OwnerId == ownerId);

        if (asNoTracking) q = q.AsNoTracking();
        return await q.ToListAsync(ct);
    }
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