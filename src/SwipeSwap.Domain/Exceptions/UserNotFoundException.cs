namespace SwipeSwap.Domain.Exceptions;

public class UserNotFoundException(int id) : BusinessException
{
    public override string Message => $"User with Id {id} not found";
}