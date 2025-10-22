namespace SwipeSwap.Application.Chat.Dtos;

public class ChatDto
{
    public int Id { get; set; }
    public int BarterId { get; set; }
    public int MessageCount { get; set; }
    public MessageDto? LastMessage { get; set; }
}
