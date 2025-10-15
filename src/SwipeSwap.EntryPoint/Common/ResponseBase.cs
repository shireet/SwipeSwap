namespace EntryPoint.Common;

public record ResponseBase
{
    public bool Successful { get; init; }
    public string? ErrorMessage { get; init; }

    protected ResponseBase()
    {
        Successful = true;
    }

    public ResponseBase(string? errorMessage)
    {
        Successful = false;
        ErrorMessage = errorMessage;
    }
}