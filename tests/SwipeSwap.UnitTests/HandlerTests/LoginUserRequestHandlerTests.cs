using FluentAssertions;
using Moq;
using SwipeSwap.Application.Auth.Login;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Jwt.Auth.Interfaces;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;
using SwipeSwap.TestInfra.Fakers;

namespace SwipeSwap.UnitTests.HandlerTests;

public class LoginUserRequestHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepo = new();
    
    [Fact]
    public async Task LoginRequestHandler_CredentialsValid_ReturnsTokens()
    {
        //Arrange
        var request = LoginUserRequestFaker.Generate();
        
        var user = UserFaker.Generate().WithEmail(request.Email).WithEncryptedSensitiveData(request.Password);
        
        var handler = new LoginRequestHandler(_userRepo.Object, _jwtService.Object, _refreshRepo.Object);

        _userRepo.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtService.Setup(x => x.GenerateToken(user)).Returns("access_token");
        _refreshRepo.Setup(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginRequestHandler_NonExistentUser_ThrowsException()
    {
        //Arrange
        var handler = new LoginRequestHandler(_userRepo.Object, _jwtService.Object, _refreshRepo.Object);
        var request = LoginUserRequestFaker.Generate();

        _userRepo.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        //Act
        var act = async () => await handler.Handle(request, CancellationToken.None);
        
        //Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginRequestHandler_InvalidPassword_ThrowsException()
    {
        //Arrange
        var user = UserFaker.Generate().WithEncryptedSensitiveData("password");
        var handler = new LoginRequestHandler(_userRepo.Object, _jwtService.Object, _refreshRepo.Object);
        var request = LoginUserRequestFaker.Generate().WithEmail(user.Email).WithPassword("WrongPassword");
        
        _userRepo.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        
        //Act
        var act = async () => await handler.Handle(request, CancellationToken.None);
        
        //Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}