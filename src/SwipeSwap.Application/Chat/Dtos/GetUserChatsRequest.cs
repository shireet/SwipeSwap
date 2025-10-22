using MediatR;

namespace SwipeSwap.Application.Chat.Dtos;

public class GetUserChatsRequest : IRequest<List<ChatDto>>
{
    public int UserId { get; set; }
}
