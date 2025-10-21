using AutoBogus;
using SwipeSwap.Domain.Models;

namespace SwipeSwap.TestInfra.Fakers;

public static class UserFaker
{
    public static User Generate()
    {
        var user = new AutoFaker<User>()
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.EncryptedSensitiveData,
                f => BCrypt.Net.BCrypt.HashPassword(f.Internet.Password(10, true, "[A-Za-z0-9!]")))
            .RuleFor(x => x.DisplayName, f => f.Person.FullName)
            .Generate();
        return user;
    }

    public static User WithEmail(this User user, string email)
    {
        user.Email = email;
        return user;
    }

    public static User WithEncryptedSensitiveData(this User user, string password)
    {
        user.EncryptedSensitiveData = BCrypt.Net.BCrypt.HashPassword(password);
        return user;
    }

    public static User WithDisplayName(this User user, string displayName)
    {
        user.DisplayName = displayName;
        return user;
    }
}