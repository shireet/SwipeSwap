namespace SwipeSwap.Domain.Exceptions;

public class EmailAlreadyInUseException(string email) : BusinessException
{
    public override string Message => $"Email {email} is already in use";
}