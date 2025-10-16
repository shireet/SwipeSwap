namespace SwipeSwap.Domain.Models;

public class Item : BaseEntity
{
    public int OwnerId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<ItemTag> ItemTags { get; set; } = [];
    public Item() { }
}