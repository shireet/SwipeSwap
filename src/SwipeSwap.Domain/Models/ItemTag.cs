namespace SwipeSwap.Domain.Models;

public class ItemTag : BaseEntity
{
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public int TagId { get; set; }
    public virtual Tag Tag { get; set; } = null!;

    public ItemTag()
    {
    }
}