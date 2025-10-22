using MediatR;

namespace SwipeSwap.Application.Chat.Dtos;

public class CreateChatRequest : IRequest<ChatDto>
{
    public int BarterId { get; set; }
    public int CreatorId { get; set; }
}
