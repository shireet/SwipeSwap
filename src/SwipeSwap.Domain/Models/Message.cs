namespace SwipeSwap.Domain.Models;

public class Message : BaseEntity
{
    public int SenderId { get; set; }
    public string? Text { get; set; }
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
    private Message() {}
}