namespace SwipeSwap.Domain.Models;

public class Tag : BaseEntity
{
    public string? Name { get; set; }
    public List<ItemTag> ItemTags { get; set; } = [];
    private Tag()
    {
    }
}