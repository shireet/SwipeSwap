using MediatR;
using SwipeSwap.Domain.Models.Enums;
using SwipeSwap.Domain.Shared;

namespace SwipeSwap.Application.Items.Dtos;

public record GetCatalogQuery(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,       
    string? SortDir = null,     
                                
    int? CategoryId = null,
    ItemCondition? Condition = null,
    string? City = null,
    string? Search = null,       
    string[]? Tags = null,       
    bool OnlyActive = true
) : IRequest<PagedResult<CatalogItem>>;
