using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwipeSwap.Application.Chat.Dtos;
using GetChatMessagesRequest = SwipeSwap.Application.Chat.GetChatMessagesRequest;
using GetUserChatsRequest = SwipeSwap.Application.Chat.GetUserChatsRequest;

namespace EntryPoint.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v1/chat")]
public class ChatController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUserChats(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var chats = await mediator.Send(new GetUserChatsRequest { UserId = userId }, ct);
        return Ok(chats);
    }

    [HttpPost]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request, CancellationToken ct)
    {
        request.CreatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var chat = await mediator.Send(request, ct);
        return Ok(chat);
    }

    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetMessages(int chatId, CancellationToken ct)
    {
        // TODO: Можно добавить проверку, что пользователь — участник
        var msgs = await mediator.Send(new GetChatMessagesRequest { ChatId = chatId }, ct);
        return Ok(msgs);
    }

    [HttpPost("{chatId}/messages")]
    public async Task<IActionResult> SendMessage(int chatId, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        request.ChatId = chatId;
        request.SenderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var msg = await mediator.Send(request, ct);
        return Ok(msg);
    }
}
