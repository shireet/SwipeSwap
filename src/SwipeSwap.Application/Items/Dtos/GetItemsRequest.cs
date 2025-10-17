// SwipeSwap.Application/Items/GetCatalogQuery.cs
using MediatR;
using SwipeSwap.Application.Dtos;
using SwipeSwap.Domain.Models.Enums;

namespace SwipeSwap.Application.Items;

public record GetCatalogQuery(
    // пагинация
    int Page = 1,
    int PageSize = 20,
    // сортировка
    string? SortBy = null,       // "price" | "created" | "title" ...
    string? SortDir = null,      // "asc" | "desc"
                                 // фильтры
    int? CategoryId = null,
    ItemCondition? Condition = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? City = null,
    string? Search = null,       // поиск по заголовку/описанию
    string[]? Tags = null,       // пересечение с тегами
    bool OnlyActive = true
) : IRequest<PagedResult<CatalogItem>>;
