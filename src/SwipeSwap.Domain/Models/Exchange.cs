using SwipeSwap.Domain.Models.Enums;

namespace SwipeSwap.Domain.Models;

public class Exchange : BaseEntity
{
    public int InitiatorId { get; private set; }   
    public int ReceiverId  { get; private set; }   

    public int OfferedItemId   { get; private set; }
    public int RequestedItemId { get; private set; }

    public string? Message { get; private set; }
    public ExchangeStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Exchange() { } 

    public static Exchange Create(int initiatorId, int receiverId, int offeredItemId, int requestedItemId, string? message)
    {
        return new Exchange
        {
            InitiatorId = initiatorId,
            ReceiverId = receiverId,
            OfferedItemId = offeredItemId,
            RequestedItemId = requestedItemId,
            Message = message,
            Status = ExchangeStatus.Sent,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Touch() => UpdatedAt = DateTime.UtcNow;
}