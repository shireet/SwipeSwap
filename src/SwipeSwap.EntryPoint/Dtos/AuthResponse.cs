namespace EntryPoint.Dtos;

public record AuthResponse : ResponseBase
{
    public string? AccessToken { get; init; } = null;
    public string? RefreshToken { get; init; } = null;
    
    public AuthResponse() {}
    public AuthResponse(string errorMessage) : base(errorMessage) {}
}