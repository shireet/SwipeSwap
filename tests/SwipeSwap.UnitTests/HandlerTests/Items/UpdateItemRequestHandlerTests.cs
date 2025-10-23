using FluentAssertions;
using Moq;
using SwipeSwap.Application.Items.Dtos;
using SwipeSwap.Application.Items.Handlers;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;


namespace SwipeSwap.UnitTests.HandlerTests.Items;

public class UpdateItemRequestHandlerTests
{
    private readonly Mock<IItemRepository> _repo = new();

    private static Item MakeItem(int id, int ownerId, string title, string? desc, bool isActive, params string[] tags)
        => new Item
        {
            Id = id,
            OwnerId = ownerId,
            Title = title,
            Description = desc,
            IsActive = isActive,
            ItemTags = tags.Select(t => new ItemTag { Tag = new Tag { Name = t } }).ToList()
        };

    [Fact(DisplayName = "Обновление: элемент не найден — возвращаем false и не сохраняем")]
    public async Task Handle_ItemNotFound_ReturnsFalse_NoSaves()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Item?)null);

        var handler = new UpdateRequestHandler(_repo.Object);
        var req = new UpdateItemRequest(
            id: 777, OwnerId: 1,
            Title: "T", Description: "D",
            IsActive: true, Tags: new List<string> { "a" });

        // Act
        var ok = await handler.Handle(req, CancellationToken.None);

        // Assert
        ok.Should().BeFalse();
        _repo.Verify(r => r.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.AddTagToItemAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.RemoveTagFromItemAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "Обновление: чужой владелец — возвращаем false, без изменений")]
    public async Task Handle_WrongOwner_ReturnsFalse_NoChanges()
    {
        // Arrange
        var dbItem = MakeItem(id: 10, ownerId: 2, title: "Old", desc: "OldD", isActive: true, "x");
        _repo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(dbItem);

        var handler = new UpdateRequestHandler(_repo.Object);
        var req = new UpdateItemRequest(
            id: 10, OwnerId: 999,
            Title: "New", Description: "NewD",
            IsActive: false, Tags: new List<string> { "y" });

        // Act
        var ok = await handler.Handle(req, CancellationToken.None);

        // Assert
        ok.Should().BeFalse();
        _repo.Verify(r => r.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.AddTagToItemAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.RemoveTagFromItemAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "Обновление: Title/Description меняются, IsActive=true применяется, теги синхронизируются")]
    public async Task Handle_UpdatesFields_ApplyIsActive_AndSyncTags()
    {
        // Arrange
        var dbItem = MakeItem(id: 5, ownerId: 1, title: "OldTitle", desc: "OldDesc", isActive: false, "a", "b");
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(dbItem);

        Item? saved = null;
        _repo.Setup(r => r.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
             .Callback<Item, CancellationToken>((i, _) => saved = i)
             .ReturnsAsync(1);

        var handler = new UpdateRequestHandler(_repo.Object);
        var req = new UpdateItemRequest(
            id: 5, OwnerId: 1,
            Title: "NewTitle", Description: "NewDesc",
            IsActive: true, Tags: new List<string> { "b", "c" } 
        );

        // Act
        var ok = await handler.Handle(req, CancellationToken.None);

        // Assert
        ok.Should().BeTrue();

        saved.Should().NotBeNull();
        saved!.Title.Should().Be("NewTitle");
        saved.Description.Should().Be("NewDesc");
        saved.IsActive.Should().BeTrue();

        _repo.Verify(r => r.AddTagToItemAsync(5, "c"), Times.Once);
        _repo.Verify(r => r.RemoveTagFromItemAsync(5, "a"), Times.Once);

        _repo.Verify(r => r.AddTagToItemAsync(5, "b"), Times.Never);      
        _repo.Verify(r => r.RemoveTagFromItemAsync(5, "b"), Times.Never);  
    }

    [Fact(DisplayName = "Обновление: IsActive = null — флаг не меняется")]
    public async Task Handle_IsActiveNull_DoesNotChangeFlag()
    {
        // Arrange
        var dbItem = MakeItem(id: 12, ownerId: 3, title: "T", desc: "D", isActive: true, "x");
        _repo.Setup(r => r.GetByIdAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(dbItem);

        Item? saved = null;
        _repo.Setup(r => r.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
             .Callback<Item, CancellationToken>((i, _) => saved = i)
             .ReturnsAsync(1);

        var handler = new UpdateRequestHandler(_repo.Object);
        var req = new UpdateItemRequest(
            id: 12, OwnerId: 3,
            Title: "T2", Description: "D2",
            IsActive: null, Tags: new List<string> { "x" }
        );

        // Act
        var ok = await handler.Handle(req, CancellationToken.None);

        // Assert
        ok.Should().BeTrue();
        saved.Should().NotBeNull();
        saved!.IsActive.Should().BeTrue(); 
    }

    [Fact(DisplayName = "Обновление: Tags = [] — удаляем все существующие теги, новых нет")]
    public async Task Handle_EmptyTags_RemovesAllExisting()
    {
        // Arrange
        var dbItem = MakeItem(id: 20, ownerId: 5, title: "T", desc: null, isActive: true, "a", "b", "c");
        _repo.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(dbItem);
        _repo.Setup(r => r.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateRequestHandler(_repo.Object);
        var req = new UpdateItemRequest(
            id: 20, OwnerId: 5,
            Title: "T", Description: null,
            IsActive: null, Tags: new List<string>()
        );

        // Act
        var ok = await handler.Handle(req, CancellationToken.None);

        // Assert
        ok.Should().BeTrue();
        _repo.Verify(r => r.AddTagToItemAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.RemoveTagFromItemAsync(20, "a"), Times.Once);
        _repo.Verify(r => r.RemoveTagFromItemAsync(20, "b"), Times.Once);
        _repo.Verify(r => r.RemoveTagFromItemAsync(20, "c"), Times.Once);
    }

    [Fact(DisplayName = "Обновление: Tags = null — теги не трогаем, только сохранение сущности")]
    public async Task Handle_NullTags_SkipsTags_OnlySave()
    {
        // Arrange
        var dbItem = MakeItem(id: 33, ownerId: 9, title: "T", desc: "D", isActive: false, "x");
        _repo.Setup(r => r.GetByIdAsync(33, It.IsAny<CancellationToken>())).ReturnsAsync(dbItem);
        _repo.Setup(r => r.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateRequestHandler(_repo.Object);

        var req = new UpdateItemRequest(
            id: 33, OwnerId: 9,
            Title: "T2", Description: "D2",
            IsActive: false, Tags: null,     
            Condition: null, City: null
        );

        // Act
        var ok = await handler.Handle(req, CancellationToken.None);

        // Assert
        ok.Should().BeTrue();
        _repo.Verify(r => r.AddTagToItemAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.RemoveTagFromItemAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.UpsertAsync(It.Is<Item>(i => i.Title == "T2" && i.Description == "D2"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Обновление: без изменения тегов — Add/Remove не вызываются, только сохранение сущности")]
    public async Task Handle_NoTagsChanges_NoAddNoRemove_OnlySave()
    {
        // Arrange
        var dbItem = MakeItem(id: 44, ownerId: 1, title: "Old", desc: "Old", isActive: true, "m", "n");
        _repo.Setup(r => r.GetByIdAsync(44, It.IsAny<CancellationToken>())).ReturnsAsync(dbItem);
        _repo.Setup(r => r.UpsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateRequestHandler(_repo.Object);
        var req = new UpdateItemRequest(
            id: 44, OwnerId: 1,
            Title: "New", Description: "New",
            IsActive: null, Tags: new List<string> { "m", "n" },
            City: null,
            Condition: null
        );

        // Act
        var ok = await handler.Handle(req, CancellationToken.None);

        // Assert
        ok.Should().BeTrue();
        _repo.Verify(r => r.AddTagToItemAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.RemoveTagFromItemAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.UpsertAsync(It.Is<Item>(i => i.Title == "New" && i.Description == "New"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
