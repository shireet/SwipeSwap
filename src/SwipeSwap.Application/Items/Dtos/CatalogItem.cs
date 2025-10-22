using SwipeSwap.Domain.Models.Enums;

namespace SwipeSwap.Application.Items.Dtos;

public record CatalogItem(
    int Id,
    string Title,
    string? Description,
    string? City,
    int? CategoryId,
    ItemCondition? Condition,
    IReadOnlyList<string> Tags
);