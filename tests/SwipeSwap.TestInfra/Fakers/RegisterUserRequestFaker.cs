using AutoBogus;
using SwipeSwap.Application.Auth.Dtos;

namespace SwipeSwap.TestInfra.Fakers;

public static class RegisterUserRequestFaker
{
    public static RegisterUserRequest Generate()
    {
        var request = new AutoFaker<RegisterUserRequest>()
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.Password,f => f.Internet.Password(10, true, "[A-Za-z0-9!]"))
            .RuleFor(x => x.DisplayName, f => f.Person.FullName)
            .Generate();
        return request;
    }

    public static RegisterUserRequest WithEmail(this RegisterUserRequest request, string email)
    {
        return request with
        {
            Email = email
        };
    }
    public static RegisterUserRequest WithPassword(this RegisterUserRequest request, string email)
    {
        return request with
        {
            Password = email
        };
    }
}