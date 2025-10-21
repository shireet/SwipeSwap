using FluentAssertions;
using Moq;
using SwipeSwap.Application.Auth.Refresh;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Jwt.Auth.Interfaces;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;
using SwipeSwap.TestInfra.Fakers;

namespace SwipeSwap.UnitTests.HandlerTests;

public class RefreshTokenHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepo = new();

    [Fact]
    public async Task RefreshTokenHandler_RefreshTokenNotExpired_GenerateNewTokens()
    {
        //Arrange
        var user = UserFaker.Generate();
        var token = RefreshTokenFaker.Generate().WithUserId(user.Id).WithExpireTime(DateTime.UtcNow.AddDays(1));
        var request = RefreshTokenRequestFaker.Generate().WithRefreshToken(token.Token);

        _refreshRepo.Setup(x => x.GetAsync(token.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        _userRepo.Setup(x => x.GetUserAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtService.Setup(x => x.GenerateToken(user)).Returns("new_access");

        var handler = new RefreshTokenHandler(_userRepo.Object, _jwtService.Object, _refreshRepo.Object);

        //Act
        var result = await handler.Handle(request, CancellationToken.None);

        //Assert
        result.AccessToken.Should().Be("new_access");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        _refreshRepo.Verify(x => x.RevokeAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _refreshRepo.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenHandler_RefreshTokenExpired_GenerateNewTokens()
    {
        //Arrange
        var user = UserFaker.Generate();
        var expired = RefreshTokenFaker.Generate().WithUserId(user.Id).WithExpireTime(DateTime.UtcNow.AddDays(-1));
        var request = RefreshTokenRequestFaker.Generate().WithRefreshToken(expired.Token);
        
        _refreshRepo.Setup(x => x.GetAsync(expired.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expired);
        var handler = new RefreshTokenHandler(_userRepo.Object, _jwtService.Object, _refreshRepo.Object);
        
        //Act
        var act = async () => await handler.Handle(request, CancellationToken.None);
        
        //Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
