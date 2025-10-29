using MediatR;
using Microsoft.EntityFrameworkCore;
using SwipeSwap.Application.Items;
using SwipeSwap.Application.Items.Dtos;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Application.Items.Handlers;

public sealed class GetRecommendedHandler(IItemRepository repo)
    : IRequestHandler<GetRecommendedQuery, IReadOnlyList<CatalogItem>>
{
    public async Task<IReadOnlyList<CatalogItem>> Handle(GetRecommendedQuery q, CancellationToken ct)
    {
        var userItems = await repo.Items
            .AsNoTracking()
            .Where(i => i.OwnerId == q.UserId && i.IsActive)
            .Select(i => new
            {
                i.CategoryId,
                i.City,
                Tags = i.ItemTags.Select(t => t.Tag!.Name)
            })
            .ToListAsync(ct);

        var userTagSet   = userItems.SelectMany(x => x.Tags).Where(t => t != null).Distinct().ToHashSet();
        var userCats     = userItems.Select(x => x.CategoryId).Where(id => id != null).Distinct().ToHashSet();
        var userCities   = userItems.Select(x => x.City).Where(c => c != null).Distinct().ToHashSet();

        var baseQuery = repo.Items.AsNoTracking()
            .Where(i => i.IsActive && i.OwnerId != q.UserId);

        if (userTagSet.Count > 0 || userCats.Count > 0 || userCities.Count > 0)
        {
            var scored = baseQuery
                .Select(i => new
                {
                    Item = i,
                    Score =
                        (userCats.Contains(i.CategoryId) ? 2 : 0) +
                        ((i.City != null && userCities.Contains(i.City)) ? 1 : 0) +
                        i.ItemTags.Count(t => userTagSet.Contains(t.Tag!.Name))
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Item.Id);

            var page = q.Page < 1 ? 1 : q.Page;
            var skip = (page - 1) * q.PageSize;

            var hits = await scored
                .Skip(skip)
                .Take(q.PageSize)
                .Select(x => ToCatalogItem(x.Item))
                .ToListAsync(ct);

            if (hits.Count > 0) return hits;
        }

        var fallback = await baseQuery
            .OrderByDescending(i => i.Id)
            .Skip((q.Page < 1 ? 0 : (q.Page - 1)) * q.PageSize)
            .Take(q.PageSize)
            .Select(i => ToCatalogItem(i))
            .ToListAsync(ct);

        return fallback;
    }

    private static CatalogItem ToCatalogItem(Item i) =>
        new(
            Id: i.Id,
            Title: i.Title,
            Description: i.Description,
            City: i.City,
            CategoryId: i.CategoryId,
            Condition: i.Condition,
            Tags: i.ItemTags
                .Select(t => t.Tag!.Name)
                .Where(n => n != null)
                .ToArray()
        );
}

public static class EfCoreMockExtensions
{
    public static IQueryable<T> AsNoTracking<T>(this IQueryable<T> source)
    {
        return source;
    }
}

