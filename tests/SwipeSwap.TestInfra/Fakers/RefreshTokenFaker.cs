using AutoBogus;
using SwipeSwap.Domain.Models;

namespace SwipeSwap.TestInfra.Fakers;

public static class RefreshTokenFaker
{
    public static RefreshToken Generate()
    {
        var refreshToken = new AutoFaker<RefreshToken>()
            .RuleFor(x => x.Revoked, f => false)
            .Generate();
        return refreshToken;
    }

    public static RefreshToken WithUserId(this RefreshToken token, int userId)
    {
        token.UserId = userId;
        return token;
    }
    
    public static RefreshToken RevokeToken(this RefreshToken token)
    {
        token.Revoked = true;
        return token;
    }

    public static RefreshToken WithExpireTime(this RefreshToken token, DateTime dateTime)
    {
        token.ExpiresAt = dateTime;
        return token;
    }
}