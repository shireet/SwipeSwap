using MediatR;
using Microsoft.EntityFrameworkCore;
using SwipeSwap.Application.Chat.Dtos;
using SwipeSwap.Infrastructure.Postgres.Context;

public class GetChatMessagesRequest : IRequest<List<MessageDto>>
{
    public int ChatId { get; set; }
}

public class GetChatMessagesHandler : IRequestHandler<GetChatMessagesRequest, List<MessageDto>>
{
    private readonly AppDbContext _db;
    public GetChatMessagesHandler(AppDbContext db) => _db = db;

    public async Task<List<MessageDto>> Handle(GetChatMessagesRequest request, CancellationToken cancellationToken)
    {
        var messages = await _db.Messages
            .AsNoTracking()
            .Where(m => m.ChatId == request.ChatId)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);
        return messages.Select(m => new MessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            Text = m.Text,
            SentAt = m.SentAt
        }).ToList();
    }
}
