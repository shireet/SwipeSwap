using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SwipeSwap.Application.Exchanges.Dtos;
using SwipeSwap.Application.Exchanges.Handlers;
using SwipeSwap.Infrastructure.Repositories.Interfaces;
using SwipeSwap.Domain.Models;
using Xunit;

namespace SwipeSwap.Application.Tests.Exchanges;

public class GetExchangeRequestHandlerTests
{
    private readonly Mock<IExchangeRepository> _repoMock = new();

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenExchangeNotFound()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var request = new GetExchangeRequest(777); 

        _repoMock
            .Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exchange?)null);

        var sut = new GetExchangeRequestHandler(_repoMock.Object);

        // Act
        var result = await sut.Handle(request, ct);

        // Assert
        result.Should().BeNull();
        _repoMock.Verify(r => r.GetByIdAsync(request.Id, ct), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapEntityToDto_WhenExchangeExists()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;

        var entity = Exchange.Create(
            initiatorId: 10,
            receiverId:  20,
            offeredItemId: 101,
            requestedItemId: 202,
            message: null
        );

        var request = new GetExchangeRequest(777);

        _repoMock
            .Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var sut = new GetExchangeRequestHandler(_repoMock.Object);

        // Act
        var dto = await sut.Handle(request, ct);

        // Assert
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(entity.Id); 
        dto.InitiatorId.Should().Be(10);
        dto.ReceiverId.Should().Be(20);
        dto.OfferedItemId.Should().Be(101);
        dto.RequestedItemId.Should().Be(202);

        var statusAsString = entity.Status.ToString();
        dto.Status.Should().Be(statusAsString);

        dto.CreatedAt.Should().BeOnOrAfter(entity.CreatedAt.AddSeconds(-1))
                     .And.BeOnOrBefore(entity.CreatedAt.AddSeconds(1));

        _repoMock.Verify(r => r.GetByIdAsync(request.Id, ct), Times.Once);
    }
}
