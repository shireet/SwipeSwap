using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SwipeSwap.Application.Items;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Repositories.Interfaces; 
using Xunit;

namespace SwipeSwap.UnitTests.HandlerTests.Items;

public class DeleteItemRequestHandlerTests
{
    private readonly Mock<IItemRepository> _itemRepo = new();

    private static Item MakeItem(int id, int ownerId, bool isActive = true) => new()
    {
        Id = id,
        OwnerId = ownerId,
        Title = $"Item {id}",
        IsActive = isActive
    };

    [Fact]
    public async Task Handle_ItemNotFound_ReturnsFalse_AndDoesNotUpsert()
    {
        // Arrange
        var itemId = 100;
        var ownerId = 1;

        _itemRepo
            .Setup(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);

        var handler = new DeleteRequestHandler(_itemRepo.Object);

        // Act
        var result = await handler.Handle(new DeleteItemRequest(itemId, ownerId), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _itemRepo.Verify(r => r.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OwnerMismatch_ReturnsFalse_AndDoesNotUpsert()
    {
        // Arrange
        var itemId = 101;
        var ownerId = 1;
        var otherOwnerId = 2;

        var dbItem = MakeItem(itemId, otherOwnerId, isActive: true);

        _itemRepo
            .Setup(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbItem);

        var handler = new DeleteRequestHandler(_itemRepo.Object);

        // Act
        var result = await handler.Handle(new DeleteItemRequest(itemId, ownerId), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _itemRepo.Verify(r => r.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OwnerMatches_SetsIsActiveFalse_CallsUpsert_ReturnsTrue()
    {
        // Arrange
        var itemId = 102;
        var ownerId = 7;

        var dbItem = MakeItem(itemId, ownerId, isActive: true);
        Item? saved = null;

        _itemRepo
            .Setup(r =>r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbItem);

        _itemRepo
            .Setup(r => r.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .Callback<Item, CancellationToken>((i, _) => saved = i)
            .ReturnsAsync(itemId);

        var handler = new DeleteRequestHandler(_itemRepo.Object);

        // Act
        var result = await handler.Handle(new DeleteItemRequest(itemId, ownerId), CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        saved.Should().NotBeNull();
        saved!.Id.Should().Be(itemId);
        saved.OwnerId.Should().Be(ownerId);
        saved.IsActive.Should().BeFalse();

        _itemRepo.Verify(r => r.UpsertAsync(It.Is<Item>(i => i.Id == itemId && i.IsActive == false), It.IsAny<CancellationToken>()), Times.Once);
    }
}
