namespace EntryPoint.Dtos;

public record AuthResponse 
{
    public string? AccessToken { get; init; } = null;
    public string? RefreshToken { get; init; } = null;
    
    public AuthResponse() {}
}