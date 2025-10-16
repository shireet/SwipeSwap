namespace SwipeSwap.Domain.Models;

public class Tag : BaseEntity
{
    public string? Name { get; set; }
    public List<ItemTag> ItemTags { get; set; } = [];
    public Tag() { }
    public Tag(string name) => Name = name;
}