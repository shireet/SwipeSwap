using SwipeSwap.Domain.Models.Enums;

namespace SwipeSwap.Domain.Models;

public class Item : BaseEntity
{
    public int OwnerId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Новые поля для каталога
    public int? CategoryId { get; set; }
    public ItemCondition? Condition { get; set; }
    public decimal? Price { get; set; }
    public string? City { get; set; }

    public List<ItemTag> ItemTags { get; set; } = [];
}
