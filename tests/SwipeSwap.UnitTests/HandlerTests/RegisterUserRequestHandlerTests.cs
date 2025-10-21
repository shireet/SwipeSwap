using Moq;
using FluentAssertions;
using SwipeSwap.Application.Auth.Register;
using SwipeSwap.Domain.Exceptions;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Jwt.Auth.Interfaces;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;
using SwipeSwap.TestInfra.Fakers;

namespace SwipeSwap.UnitTests.HandlerTests;

public class RegisterRequestHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepo = new();

    [Fact]
    public async Task RegisterUserHandler_SendNonExistentUser_ReturnsTokens()
    {
        // Arrange
        var handler = new RegisterRequestHandler(_userRepo.Object, _jwtService.Object, _refreshRepo.Object);
        var request = RegisterUserRequestFaker.Generate();
            

        _userRepo.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _jwtService.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("access");
        _refreshRepo.Setup(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("access");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        _userRepo.Verify(x => x.UpsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserHandler_EmailAlreadyExists_ThrowsException()
    {
        var existingUser = UserFaker.Generate();
        var handler = new RegisterRequestHandler(_userRepo.Object, _jwtService.Object, _refreshRepo.Object);

        _userRepo.Setup(x => x.GetByEmailAsync(existingUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var request = RegisterUserRequestFaker.Generate().WithEmail(existingUser.Email);

        // Act
        var act = async () => await handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EmailAlreadyInUseException>();
    }
}
