using MediatR;
using Microsoft.EntityFrameworkCore;
using SwipeSwap.Application.Chat.Dtos;
using SwipeSwap.Infrastructure.Postgres.Context;
using SwipeSwap.Domain.Models;

public class SendMessageHandler : IRequestHandler<SendMessageRequest, MessageDto>
{
    private readonly AppDbContext _db;
    public SendMessageHandler(AppDbContext db) => _db = db;

    public async Task<MessageDto> Handle(SendMessageRequest request, CancellationToken cancellationToken)
    {
        var chat = await _db.Chats.Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);
        if (chat is null)
            throw new InvalidOperationException("Чат не найден");
        
        var message = new Message {
            SenderId = request.SenderId,
            Text = request.Text,
            SentAt = DateTimeOffset.UtcNow
        };
        chat.Messages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);
        return new MessageDto {
            Id = message.Id,
            SenderId = message.SenderId,
            Text = message.Text,
            SentAt = message.SentAt
        };
    }
}
