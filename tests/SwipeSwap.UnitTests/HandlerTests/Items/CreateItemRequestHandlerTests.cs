using FluentAssertions;
using Moq;
using SwipeSwap.Application.Items.Dtos;
using SwipeSwap.Application.Items.Handlers;
using SwipeSwap.Domain.Models;
using SwipeSwap.Domain.Models.Enums;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.UnitTests.HandlerTests.Items;

public class CreateItemRequestHandlerTests
{
    private readonly Mock<IItemRepository> _itemRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();

    private static User MakeUser(int id) => new User
    {
        Id = id,
        Email = $"user{id}@example.com",
        DisplayName = $"User {id}",
        EncryptedSensitiveData = string.Empty
    };

    [Fact]
    public async Task Handle_OwnerExists_SavesItem_WithImageUrl_AndReturnsId()
    {
        // Arrange
        var ownerId = 42;
        var imageUrl = "https://cdn.example.com/img/42.jpg";

        _userRepo
            .Setup(x => x.GetUserAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(ownerId));

        var expectedId = 123;
        Item? saved = null;

        _itemRepo
            .Setup(x => x.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .Callback<Item, CancellationToken>((it, _) => saved = it)
            .ReturnsAsync(expectedId);

        var handler = new CreateRequestHandler(_itemRepo.Object, _userRepo.Object);

        var req = new CreateItemRequest(
            OwnerId: ownerId,
            Title: "Mountain Bike",
            ImageUrl: imageUrl,
            Description: "Good condition",
            Tags: new List<string> { "sport", "bike" },
            Condition: ItemCondition.Fair,
            City: "Волгоград"        
        );

        // Act
        var id = await handler.Handle(req, CancellationToken.None);

        // Assert
        id.Should().Be(expectedId);

        saved.Should().NotBeNull();
        var i = saved!;

        i.OwnerId.Should().Be(ownerId);
        i.Title.Should().Be(req.Title);
        i.Description.Should().Be(req.Description);
        i.ImageUrl.Should().Be(imageUrl);
        i.ItemTags.Select(t => t.Tag.Name).OrderBy(n => n)
            .Should().Equal("bike", "sport");
    }

    [Fact]
    public async Task Handle_OwnerDoesNotExist_ThrowsInvalidOperationException_AndDoesNotCallUpsert()
    {
        // Arrange
        _userRepo
            .Setup(x => x.GetUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new CreateRequestHandler(_itemRepo.Object, _userRepo.Object);

        var req = new CreateItemRequest(
            OwnerId: 999,
            Title: "Anything",
            ImageUrl: "https://x/any.jpg",
            Description: "Doesn't matter",
            Tags: new List<string> { "tag" },
            Condition: ItemCondition.Good,
            City: "Санкт-Петербург"        
        );

        // Act
        var act = async () => await handler.Handle(req, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Пользователь не существует.");

        _itemRepo.Verify(x => x.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NormalizesTags_Trims_FiltersEmptyAndDuplicates_IgnoringCase()
    {
        // Arrange
        var ownerId = 7;

        _userRepo
            .Setup(x => x.GetUserAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(ownerId));

        Item? capturedItem = null;
        _itemRepo
            .Setup(x => x.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .Callback<Item, CancellationToken>((it, _) => capturedItem = it)
            .ReturnsAsync(1);

        var handler = new CreateRequestHandler(_itemRepo.Object, _userRepo.Object);

        var req = new CreateItemRequest(
            OwnerId: ownerId,
            Title: "Title",
            ImageUrl: "https://cdn/img.jpg",
            Description: "Desc",
            Tags: new List<string?>
            {
                "  TagOne ",
                "tagone",
                "TAGTWO",
                "",
                "   ",
                null,
                "TagTwo "
            }!.Where(s => s is not null).Select(s => s!).ToList(),
            Condition: ItemCondition.LikeNew,
            City: "Казань"        
        );

        // Act
        _ = await handler.Handle(req, CancellationToken.None);

        // Assert
        capturedItem.Should().NotBeNull();
        var i = capturedItem!;
        var names = i.ItemTags.Select(t => t.Tag.Name).ToList();

        names.Should().HaveCount(2);
        names.Should().OnlyContain(n => n == n.Trim());
        names.Select(n => n.ToLowerInvariant()).Should().BeEquivalentTo(new[] { "tagone", "tagtwo" });
    }

    [Fact]
    public async Task Handle_NullTags_SavesWithNoTags_AndKeepsImageUrl()
    {
        // Arrange
        var ownerId = 10;
        _userRepo
            .Setup(x => x.GetUserAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(ownerId));

        Item? saved = null;
        _itemRepo
            .Setup(x => x.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .Callback<Item, CancellationToken>((i, _) => saved = i)
            .ReturnsAsync(5);

        var handler = new CreateRequestHandler(_itemRepo.Object, _userRepo.Object);

        var req = new CreateItemRequest(
            OwnerId: ownerId,
            Title: "No tags item",
            ImageUrl: "https://cdn/no-tags.jpg",
            Description: "Empty tags",
            Tags: null!,
            Condition: ItemCondition.Poor,
            City: "Новосибирск"        
        );

        // Act
        var id = await handler.Handle(req, CancellationToken.None);

        // Assert
        id.Should().Be(5);

        saved.Should().NotBeNull();
        var i = saved!;
        i.ItemTags.Should().NotBeNull().And.BeEmpty();
        i.ImageUrl.Should().Be("https://cdn/no-tags.jpg");
    }
}
