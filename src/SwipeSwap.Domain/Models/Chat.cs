namespace SwipeSwap.Domain.Models;

public class Chat : BaseEntity
{
    public int BarterId { get; set; }
    public List<Message> Messages = [];
}