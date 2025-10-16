namespace EntryPoint.Dtos.items
{
    public class UpdateItem
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public bool? IsActive { get; init; }
        public List<string>? Tags { get; init; }
    }
}
