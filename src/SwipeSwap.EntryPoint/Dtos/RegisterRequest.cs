namespace EntryPoint.Dtos;

public record RegisterRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string DisplayName { get; init; }
}