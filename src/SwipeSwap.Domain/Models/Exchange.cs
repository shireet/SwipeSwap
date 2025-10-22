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
    
    public void Accept(int actorUserId)
    {
        Ensure(Status == ExchangeStatus.Sent, "Можно принять только предложение в статусе Sent.");
        Ensure(actorUserId == ReceiverId, "Принять предложение может только получатель.");
        Status = ExchangeStatus.Accepted;
        Touch();
    }

    public void Decline(int actorUserId, string? reason = null)
    {
        Ensure(Status == ExchangeStatus.Sent, "Отклонить можно только предложение в статусе Sent.");
        Ensure(actorUserId == ReceiverId, "Отклонить предложение может только получатель.");
        Status = ExchangeStatus.Declined;
        if (!string.IsNullOrWhiteSpace(reason))
            Message = $"{Message}\n[Decline]: {reason}";
        Touch();
    }

    public void Cancel(int actorUserId, string? reason = null)
    {
        Ensure(Status is ExchangeStatus.Sent or ExchangeStatus.Accepted, 
            "Отменить можно только предложение в статусе Sent или Accepted.");
        Ensure(actorUserId == InitiatorId || actorUserId == ReceiverId,
            "Отменить может только участник обмена.");
        Status = ExchangeStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
            Message = $"{Message}\n[Cancel]: {reason}";
        Touch();
    }

    public void Complete(int actorUserId, string? note = null)
    {
        Ensure(Status == ExchangeStatus.Accepted, "Завершить можно только принятый обмен.");
        Ensure(actorUserId == InitiatorId, "Завершить обмен может только инициатор (по умолчанию).");
        Status = ExchangeStatus.Completed;
        if (!string.IsNullOrWhiteSpace(note))
            Message = $"{Message}\n[Complete]: {note}";
        Touch();
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}