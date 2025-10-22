using FluentAssertions;
using Moq;
using SwipeSwap.Application.Profile;
using SwipeSwap.Application.Profile.Dtos;
using SwipeSwap.Domain.Exceptions;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Jwt.Services.Interfaces;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;
using SwipeSwap.TestInfra.Fakers;

namespace SwipeSwap.UnitTests.HandlerTests;

public class UpdateCurrentUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ICurrentUserService> _current = new();

    [Fact]
    public async Task UpdateCurrentUserHandler_UpdateDisplayName()
    {
        // Arrange
        var user = UserFaker.Generate();
        var newName = "NewName";
        _current.Setup(x => x.UserId).Returns(user.Id);
        _userRepo.Setup(x => x.GetUserAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user.WithDisplayName(newName));

        var handler = new UpdateCurrentUserHandler(_userRepo.Object, _current.Object);
        
        //Act
        var result = await handler.Handle(new UpdateUserRequest{DisplayName = newName}, CancellationToken.None);

        //Assert
        result.DisplayName.Should().Be(newName);
        _userRepo.Verify(x => x.UpsertAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCurrentUserHandler_UserNotFound_ThrowsException()
    {
        //Arrange
        var userId = 123;
        _current.Setup(x => x.UserId).Returns(userId);
        _userRepo.Setup(x => x.GetUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var handler = new UpdateCurrentUserHandler(_userRepo.Object, _current.Object);
        
        //Act
        var act = async () => await handler.Handle(new UpdateUserRequest{DisplayName = "NewName"}, CancellationToken.None);
        
        //Assert
        await act.Should().ThrowAsync<UserNotFoundException>();
    }
}
