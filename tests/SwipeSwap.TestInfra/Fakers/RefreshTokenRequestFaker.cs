using AutoBogus;
using SwipeSwap.Application.Auth.Dtos;

namespace SwipeSwap.TestInfra.Fakers;

public static class RefreshTokenRequestFaker
{
    public static RefreshTokenRequest Generate()
    {
        var request = new AutoFaker<RefreshTokenRequest>().Generate();
        return request;
    }
    public static RefreshTokenRequest WithRefreshToken(this RefreshTokenRequest request, string refreshToken)
    {
        return new RefreshTokenRequest(refreshToken);
    }
}