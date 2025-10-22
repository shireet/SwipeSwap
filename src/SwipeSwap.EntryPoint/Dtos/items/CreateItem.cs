namespace EntryPoint.Dtos.items;

public class CreateItem
{
    public required string Title { get; init; }      
    public string? Description { get; init; }
    
    public string ImageUrl { get; init; }
    public List<string>? Tags { get; init; }
}