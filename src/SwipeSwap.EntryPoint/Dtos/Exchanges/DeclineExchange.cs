namespace SwipeSwap.EntryPoint.Dtos.Exchanges;

public sealed record DeclineExchange(int ActorUserId, string Reason, string? Note);