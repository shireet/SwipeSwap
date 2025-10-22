namespace SwipeSwap.Application.Chat.Dtos;

public class MessageDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string? Text { get; set; }
    public DateTimeOffset SentAt { get; set; }
}
