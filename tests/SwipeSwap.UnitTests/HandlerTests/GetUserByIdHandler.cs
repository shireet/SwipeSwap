using FluentAssertions;
using Moq;
using SwipeSwap.Application.Profile;
using SwipeSwap.Application.Profile.Dtos;
using SwipeSwap.Domain.Exceptions;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;
using SwipeSwap.TestInfra.Fakers;

namespace SwipeSwap.UnitTests.HandlerTests;

public class GetUserByIdHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();

    [Fact]
    public async Task GetUSerByIdHandler_UserFound_ReturnUser()
    {
        //Arrange
        var user = UserFaker.Generate();
        _userRepo.Setup(x => x.GetUserAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = new GetUserByIdHandler(_userRepo.Object);
        
        //Act
        var result = await handler.Handle(new GetUserByIdRequest(user.Id), CancellationToken.None);

        //Assert
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetUSerByIdHandler_UserNotFound_ThrowsException()
    {
        //Arrange
        var user =  UserFaker.Generate();
        _userRepo.Setup(x => x.GetUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new GetUserByIdHandler(_userRepo.Object);
        
        //Act
        var act = async () => await handler.Handle(new GetUserByIdRequest(user.Id), CancellationToken.None);
        
        //Assert
        await act.Should().ThrowAsync<UserNotFoundException>();
    }
}