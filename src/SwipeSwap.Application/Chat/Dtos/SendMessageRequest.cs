using MediatR;

namespace SwipeSwap.Application.Chat.Dtos;

public class SendMessageRequest : IRequest<MessageDto>
{
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public string Text { get; set; } = null!;
}
