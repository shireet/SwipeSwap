namespace SwipeSwap.EntryPoint.Dtos.Exchanges;

public sealed record CancelExchange(int ActorUserId, string? Reason, string? Note);