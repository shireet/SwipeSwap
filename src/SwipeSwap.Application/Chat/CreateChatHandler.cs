using MediatR;
using Microsoft.EntityFrameworkCore;
using SwipeSwap.Application.Chat.Dtos;
using SwipeSwap.Infrastructure.Postgres.Context;
using SwipeSwap.Domain.Models;

public class CreateChatHandler : IRequestHandler<CreateChatRequest, ChatDto>
{
    private readonly AppDbContext _db;
    public CreateChatHandler(AppDbContext db) => _db = db;

    public async Task<ChatDto> Handle(CreateChatRequest request, CancellationToken cancellationToken)
    {
        var chat = await _db.Chats.Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.BarterId == request.BarterId, cancellationToken);
        if (chat is null)
        {
            chat = new Chat { BarterId = request.BarterId };
            _db.Chats.Add(chat);
            await _db.SaveChangesAsync(cancellationToken);
        }
        return new ChatDto {
            Id = chat.Id,
            BarterId = chat.BarterId,
            MessageCount = chat.Messages.Count,
            LastMessage = chat.Messages
                .OrderByDescending(m => m.SentAt)
                .Select(m => new MessageDto {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    Text = m.Text,
                    SentAt = m.SentAt
                }).FirstOrDefault()
        };
    }
}
