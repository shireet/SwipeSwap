using MediatR;
using Microsoft.EntityFrameworkCore;
using SwipeSwap.Application.Chat.Dtos;
using SwipeSwap.Infrastructure.Postgres.Context;

public class GetUserChatsRequest : IRequest<List<ChatDto>>
{
    public int UserId { get; set; }
}

public class GetUserChatsHandler : IRequestHandler<GetUserChatsRequest, List<ChatDto>>
{
    private readonly AppDbContext _db;
    public GetUserChatsHandler(AppDbContext db) => _db = db;

    public async Task<List<ChatDto>> Handle(GetUserChatsRequest request, CancellationToken cancellationToken)
    {
        var barterIds = await _db.Exchanges
            .Where(x => x.InitiatorId == request.UserId || x.ReceiverId == request.UserId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var chats = await _db.Chats
            .Include(c => c.Messages)
            .Where(c => barterIds.Contains(c.BarterId))
            .ToListAsync(cancellationToken);

        return chats
            .Select(c => new ChatDto
            {
                Id = c.Id,
                BarterId = c.BarterId,
                MessageCount = c.Messages.Count,
                LastMessage = c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => new MessageDto {
                        Id = m.Id,
                        SenderId = m.SenderId,
                        Text = m.Text,
                        SentAt = m.SentAt
                    }).FirstOrDefault()
            })
            .ToList();
    }
}
