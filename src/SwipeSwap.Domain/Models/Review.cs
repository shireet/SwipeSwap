namespace SwipeSwap.Domain.Models;

public class Review : BaseEntity
{
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public int BarterId { get; set; }
    public int Rating { get; set; } 
    public string? Text { get; set; }
    private Review(){}
}