using SwipeSwap.Domain.Models.Enums;

namespace SwipeSwap.Domain.Models;

public class Barter : BaseEntity
{
    public int ItemAId { get; set; }
    public int ItemBId { get; set; }
    public int InitiatorUserId { get; set; }
    public BarterStatus Status { get; set; } = BarterStatus.Pending;
    public int ChatId { get; set; }
    
    private Barter() {}
}