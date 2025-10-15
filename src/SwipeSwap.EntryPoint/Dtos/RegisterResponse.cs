using EntryPoint.Common;

namespace EntryPoint.Dtos;

public record RegisterResponse : ResponseBase
{
    public string? Token { get; init; }
    
    public RegisterResponse(string? errorMessage) : base(errorMessage) { }
    
    public RegisterResponse() {}
}