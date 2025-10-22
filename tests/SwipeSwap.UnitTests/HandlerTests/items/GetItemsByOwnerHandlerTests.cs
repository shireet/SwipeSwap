using FluentAssertions;
using Moq;
using SwipeSwap.Application.Items.Dtos;
using SwipeSwap.Application.Items;
using SwipeSwap.Application.Items.Handlers;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Repositories.Interfaces; 
using Xunit;

namespace SwipeSwap.UnitTests.HandlerTests.Items;

public class GetItemsByOwnerHandlerTests
{
    private readonly Mock<IItemRepository> _repo = new();

    private static Item MakeItem(
        int id,
        int ownerId,
        string title,
        string? description,
        params string[] tags)
    {
        return new Item
        {
            Id = id,
            OwnerId = ownerId,
            Title = title,
            Description = description,
            ItemTags = tags.Select(t => new ItemTag { Tag = new Tag { Name = t } }).ToList()
        };
    }

    [Fact]
    public async Task Handle_OwnerHasItems_MapsAllFields_ForEachItem()
    {
        // Arrange
        const int ownerId = 101;

        var entities = new List<Item>
        {
            MakeItem(1, ownerId, "Bike", "MTB", "sport", "bike"),
            MakeItem(2, ownerId, "Laptop", "Ultrabook", "electronics")
        };

        _repo.Setup(r => r.GetByOwnerAsync(ownerId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(entities);

        var handler = new GetItemsByOwnerHandler(_repo.Object);
        var req = new GetItemsByOwnerRequest(ownerId);

        // Act
        var dtos = await handler.Handle(req, CancellationToken.None);

        // Assert
        dtos.Should().NotBeNull().And.HaveCount(2);

        dtos[0].Should().BeEquivalentTo(new ItemDto(
            Id: 1,
            OwnerId: ownerId,
            Title: "Bike",
            Description: "MTB",
            Tags: new List<string> { "sport", "bike" }
        ));

        dtos[1].Should().BeEquivalentTo(new ItemDto(
            Id: 2,
            OwnerId: ownerId,
            Title: "Laptop",
            Description: "Ultrabook",
            Tags: new List<string> { "electronics" }
        ));

        _repo.Verify(r => r.GetByOwnerAsync(ownerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OwnerHasNoItems_ReturnsEmptyList()
    {
        // Arrange
        const int ownerId = 202;
        _repo.Setup(r => r.GetByOwnerAsync(ownerId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Item>());

        var handler = new GetItemsByOwnerHandler(_repo.Object);

        // Act
        var dtos = await handler.Handle(new GetItemsByOwnerRequest(ownerId), CancellationToken.None);

        // Assert
        dtos.Should().NotBeNull().And.BeEmpty();
        _repo.Verify(r => r.GetByOwnerAsync(ownerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ItemWithNoTags_AndNullDescription_ProducesEmptyTagsAndNullDescription()
    {
        // Arrange
        const int ownerId = 303;
        var entity = MakeItem(10, ownerId, "No tags", null);
        entity.ItemTags.Clear(); 

        _repo.Setup(r => r.GetByOwnerAsync(ownerId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Item> { entity });

        var handler = new GetItemsByOwnerHandler(_repo.Object);

        // Act
        var dtos = await handler.Handle(new GetItemsByOwnerRequest(ownerId), CancellationToken.None);

        // Assert
        dtos.Should().HaveCount(1);
        var dto = dtos[0];

        dto.Id.Should().Be(entity.Id);
        dto.OwnerId.Should().Be(ownerId);
        dto.Title.Should().Be("No tags");
        dto.Description.Should().BeNull();
        dto.Tags.Should().NotBeNull().And.BeEmpty();

        _repo.Verify(r => r.GetByOwnerAsync(ownerId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
