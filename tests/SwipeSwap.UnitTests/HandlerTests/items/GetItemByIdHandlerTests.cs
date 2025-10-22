using FluentAssertions;
using Moq;
using SwipeSwap.Application.Items.Dtos;
using SwipeSwap.Application.Items.Handlers;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.UnitTests.HandlerTests.Items;

public class GetItemByIdHandlerTests
{
    private readonly Mock<IItemRepository> _repo = new();

    private static Item MakeItem(
        int id,
        int ownerId,
        string title = "Title",
        string? description = "Desc",
        params string[] tags)
    {
        return new Item
        {
            Id = id,
            OwnerId = ownerId,
            Title = title,
            Description = description,
            ItemTags = tags
                .Select(t => new ItemTag { Tag = new Tag { Name = t } })
                .ToList()
        };
    }

    [Fact]
    public async Task Handle_ItemExists_MapsAllFields_AndReturnsDto()
    {
        // Arrange
        var entity = MakeItem(id: 5, ownerId: 77, title: "Bike", description: "MTB", "sport", "bike");

        _repo.Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var handler = new GetItemByIdHandler(_repo.Object);
        var query = new GetItemByIdQuery(entity.Id);

        // Act
        var dto = await handler.Handle(query, CancellationToken.None);

        // Assert
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(entity.Id);
        dto.OwnerId.Should().Be(entity.OwnerId);
        dto.Title.Should().Be(entity.Title);
        dto.Description.Should().Be(entity.Description);
        dto.Tags.Should().Equal("sport", "bike");

        _repo.Verify(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ItemNotFound_ReturnsNull()
    {
        // Arrange
        const int id = 404;
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Item?)null);

        var handler = new GetItemByIdHandler(_repo.Object);

        // Act
        var dto = await handler.Handle(new GetItemByIdQuery(id), CancellationToken.None);

        // Assert
        dto.Should().BeNull();
        _repo.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ItemWithoutTags_ReturnsEmptyTagsList()
    {
        // Arrange
        var entity = MakeItem(id: 10, ownerId: 1, title: "No tags", description: null);
        entity.ItemTags.Clear();

        _repo.Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var handler = new GetItemByIdHandler(_repo.Object);

        // Act
        var dto = await handler.Handle(new GetItemByIdQuery(entity.Id), CancellationToken.None);

        // Assert
        dto.Should().NotBeNull();
        dto!.Description.Should().BeNull();
        dto.Tags.Should().NotBeNull().And.BeEmpty();

        _repo.Verify(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
