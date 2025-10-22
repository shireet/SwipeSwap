namespace EntryPoint.Dtos;

public record UpdateUserRequest
{
    public required string DisplayName { get; init; }
}