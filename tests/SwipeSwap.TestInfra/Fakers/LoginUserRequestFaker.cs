using AutoBogus;
using SwipeSwap.Application.Auth.Dtos;

namespace SwipeSwap.TestInfra.Fakers;

public static class LoginUserRequestFaker
{
    public static LoginUserRequest Generate()
    {
        var request = new AutoFaker<LoginUserRequest>()
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.Password, f => f.Internet.Password(10, true, "[A-Za-z0-9!]"))
            .Generate();
        return request;
    }

    public static LoginUserRequest WithEmail(this LoginUserRequest request, string email)
    {
        return request with
        {
            Email = email
        };
    }

    public static LoginUserRequest WithPassword(this LoginUserRequest request, string password)
    {
        return request with
        {
            Password = password
        };
    }
}