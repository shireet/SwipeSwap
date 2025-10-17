

using SwipeSwap.Domain.Models.Enums;

namespace SwipeSwap.Application.Dtos;

public record CatalogItem(
    int Id,
    string Title,
    string? Description,
    decimal? Price,
    string? City,
    int? CategoryId,
    ItemCondition? Condition,
    IReadOnlyList<string> Tags
);
