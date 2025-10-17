// SwipeSwap.Application/Items/GetCatalogHandler.cs
using MediatR;
using SwipeSwap.Application.Dtos;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Application.Items;

public class GetCatalogHandler(IItemRepository repo)
    : IRequestHandler<GetCatalogQuery, PagedResult<CatalogItem>>
{
    public async Task<PagedResult<CatalogItem>> Handle(GetCatalogQuery q, CancellationToken ct)
    {
        var page = await repo.GetCatalogAsync(
            q.Page, q.PageSize, q.SortBy, q.SortDir,
            q.CategoryId, q.Condition, q.MinPrice, q.MaxPrice,
            q.City, q.Search, q.Tags, q.OnlyActive, ct);

        var dto = page.Items.Select(i => new CatalogItem(
            i.Id,
            i.Title,
            i.Description,
            i.Price,
            i.City,
            i.CategoryId,
            i.Condition,
            i.ItemTags.Select(t => t.Tag!.Name).ToArray()
        )).ToList();

        return new PagedResult<CatalogItem>(dto, page.Page, page.PageSize, page.Total);
    }
}
