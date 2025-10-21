using FluentAssertions;
using Moq;
using SwipeSwap.Application.Profile;
using SwipeSwap.Application.Profile.Dtos;
using SwipeSwap.Domain.Exceptions;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;
using SwipeSwap.TestInfra.Fakers;

namespace SwipeSwap.UnitTests.HandlerTests;

public class GetCurrentUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();

    [Fact]
    public async Task GetCurrentUserHandler_ReturnsCurrentUser()
    {
        //Arrange
        var user = UserFaker.Generate();
        var request = new GetCurrentUserRequest(user.Id);
        
        _userRepo.Setup(x => x.GetUserAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = new GetCurrentUserHandler(_userRepo.Object);
        
        //Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        result.Email.Should().Be(user.Email);
        result.DisplayName.Should().Be(user.DisplayName);
    }

    [Fact]
    public async Task GetCurrentUserHandler_UserNotFound_ThrowsException()
    {
        //Arrange
        _userRepo.Setup(x => x.GetUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var handler = new GetCurrentUserHandler(_userRepo.Object);
        
        //Act
        var act = async () => await handler.Handle(new GetCurrentUserRequest(999), CancellationToken.None);
        
        //Arrange
        await act.Should().ThrowAsync<UserNotFoundException>();
    }
}