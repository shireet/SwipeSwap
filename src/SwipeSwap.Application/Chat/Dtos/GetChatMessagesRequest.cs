using MediatR;

namespace SwipeSwap.Application.Chat.Dtos;

public class GetChatMessagesRequest : IRequest<List<MessageDto>>
{
    public int ChatId { get; set; }
}
