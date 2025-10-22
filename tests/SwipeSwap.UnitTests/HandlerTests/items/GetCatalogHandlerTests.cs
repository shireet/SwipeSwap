using FluentAssertions;
using Moq;
using SwipeSwap.Application.Items.Dtos;                
using SwipeSwap.Application.Items.Handlers;
using SwipeSwap.Domain.Models;                   
using SwipeSwap.Domain.Models.Enums;             
using SwipeSwap.Domain.Shared;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;
using Xunit;

namespace SwipeSwap.UnitTests.HandlerTests.Items;

public class GetCatalogHandlerTests
{
    private readonly Mock<IItemRepository> _repo = new();

    private static Item MakeItem(
        int id,
        string title,
        string? description,
        string? city,
        int? categoryId,
        ItemCondition? condition,
        params string[] tags)
    {
        return new Item
        {
            Id = id,
            OwnerId = 0,
            Title = title,
            Description = description,
            City = city,
            CategoryId = categoryId,
            Condition = condition,
            ItemTags = tags.Select(t => new ItemTag { Tag = new Tag { Name = t } }).ToList()
        };
    }
    
        [Fact(DisplayName = "Каталог: OnlyActive=false + сортировка и все фильтры пробрасываются как есть")]
    public async Task Handle_OnlyActiveFalse_WithSortingAndAllFilters_ForwardedExactly()
    {
        // Arrange
        var q = new GetCatalogQuery(
            Page: 3,
            PageSize: 15,
            SortBy: "createdAt",
            SortDir: "asc",
            CategoryId: 9,
            Condition: ItemCondition.Fair,
            City: "Paris",
            Search: "roller",
            Tags: new[] { "skate", "sport" },
            OnlyActive: false
        );

        var items = new List<Item>
        {
            new Item
            {
                Id = 100, OwnerId = 1, Title = "Roller A", Description = "A",
                City = "Paris", CategoryId = 9, Condition = ItemCondition.Fair,
                ItemTags = new List<ItemTag>
                {
                    new() { Tag = new Tag { Name = "skate" } },
                    new() { Tag = new Tag { Name = "sport" } },
                }
            }
        };

        var page = new PagedResult<Item>(items, totalCount: 25, page: 3, pageSize: 15);

        var repo = new Mock<IItemRepository>();
        repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition,
                q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(page);

        var handler = new GetCatalogHandler(repo.Object);

        // Act
        var res = await handler.Handle(q, CancellationToken.None);

        // Assert
        res.TotalCount.Should().Be(25);
        res.Page.Should().Be(3);
        res.PageSize.Should().Be(15);

        res.Items.Should().ContainSingle();
        res.Items[0].Should().BeEquivalentTo(new CatalogItem(
            Id: 100, Title: "Roller A", Description: "A", City: "Paris",
            CategoryId: 9, Condition: ItemCondition.Fair, Tags: new[] { "skate", "sport" }
        ));

        // Verify 
        repo.Verify(r => r.GetCatalogAsync(
            3, 15, "createdAt", "asc", 9, ItemCondition.Fair, "Paris", "roller",
            It.Is<string[]>(t => t.SequenceEqual(new[] { "skate", "sport" })), false,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Каталог: элемент с множеством тегов — все теги попадают в DTO в том же порядке")]
    public async Task Handle_ItemWithManyTags_MapsAllTags_PreservingOrder()
    {
        // Arrange
        var q = new GetCatalogQuery(1, 10, null, null, null, null, null, null, null, true);

        var item = new Item
        {
            Id = 77, OwnerId = 2, Title = "Multi-tags", Description = "D",
            City = "City", CategoryId = 1, Condition = ItemCondition.Fair,
            ItemTags = new List<ItemTag>
            {
                new() { Tag = new Tag { Name = "alpha" } },
                new() { Tag = new Tag { Name = "Beta" } },
                new() { Tag = new Tag { Name = "GAMMA" } },
                new() { Tag = new Tag { Name = "beta" } },
            }
        };

        var page = new PagedResult<Item>(new List<Item> { item }, 1, 1, 10);

        var repo = new Mock<IItemRepository>();
        repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition, q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(page);

        var handler = new GetCatalogHandler(repo.Object);

        // Act
        var res = await handler.Handle(q, CancellationToken.None);

        // Assert
        res.Items.Should().ContainSingle();
        res.Items[0].Tags.Should().Equal("alpha", "Beta", "GAMMA", "beta");
    }

    [Fact(DisplayName = "Каталог: большая пагинация (PageSize=100) — хэндлер возвращает ровно то, что прислал репозиторий")]
    public async Task Handle_LargePaging_ReturnsExactlyWhatRepoReturns()
    {
        // Arrange
        var q = new GetCatalogQuery(5, 100, null, null, null, null, null, null, null, true);

        var items = Enumerable.Range(1, 100).Select(i => new Item
        {
            Id = i, OwnerId = 1, Title = $"T{i}", Description = null,
            City = null, CategoryId = null, Condition = null,
            ItemTags = new List<ItemTag>()
        }).ToList();

        var page = new PagedResult<Item>(items, totalCount: 1000, page: 5, pageSize: 100);

        var repo = new Mock<IItemRepository>();
        repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition, q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(page);

        var handler = new GetCatalogHandler(repo.Object);

        // Act
        var res = await handler.Handle(q, CancellationToken.None);

        // Assert
        res.Items.Should().HaveCount(100);
        res.TotalCount.Should().Be(1000);
        res.Page.Should().Be(5);
        res.PageSize.Should().Be(100);
    }

    [Fact(DisplayName = "Каталог: пробельные значения в City/Search пробрасываются как есть")]
    public async Task Handle_WhitespaceCityAndSearch_ForwardedAsIs()
    {
        // Arrange
        var q = new GetCatalogQuery(
            Page: 1, PageSize: 10,
            SortBy: null, SortDir: null,
            CategoryId: null, Condition: null,
            City: "   ", Search: "\t",
            Tags: null, OnlyActive: true
        );

        var page = new PagedResult<Item>(new List<Item>(), 0, 1, 10);

        var repo = new Mock<IItemRepository>();
        repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition, q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(page);

        var handler = new GetCatalogHandler(repo.Object);

        // Act
        _ = await handler.Handle(q, CancellationToken.None);

        // Assert 
        repo.Verify(r => r.GetCatalogAsync(
            1, 10, null, null, null, null,
            "   ", "\t", null, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Каталог: регистр тегов сохраняется (чувствительность к регистру — как отдал репозиторий)")]
    public async Task Handle_TagsCaseIsPreserved()
    {
        // Arrange
        var q = new GetCatalogQuery(1, 10, null, null, null, null, null, null, null, true);

        var item = new Item
        {
            Id = 9, OwnerId = 1, Title = "Case", Description = null,
            City = null, CategoryId = null, Condition = null,
            ItemTags = new List<ItemTag>
            {
                new() { Tag = new Tag { Name = "CamelCase" } },
                new() { Tag = new Tag { Name = "lower" } },
                new() { Tag = new Tag { Name = "UPPER" } },
            }
        };

        var page = new PagedResult<Item>(new List<Item> { item }, 1, 1, 10);

        var repo = new Mock<IItemRepository>();
        repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition, q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(page);

        var handler = new GetCatalogHandler(repo.Object);

        // Act
        var res = await handler.Handle(q, CancellationToken.None);

        // Assert 
        res.Items.Should().ContainSingle();
        res.Items[0].Tags.Should().Equal("CamelCase", "lower", "UPPER");
    }
    
    [Fact(DisplayName = "Общий каталог без фильтров: OnlyActive=true, сортировка/поиск/tags/city/category/condition = null")]
    public async Task Handle_GeneralCatalog_NoFilters_DefaultOnlyActiveTrue()
    {
        // Arrange 
        var q = new GetCatalogQuery(
            Page: 1,
            PageSize: 20,
            SortBy: null,
            SortDir: null,
            CategoryId: null,
            Condition: null,
            City: null,
            Search: null,
            Tags: null,
            OnlyActive: true 
        );

        var items = new List<Item>
        {
            new Item
            {
                Id = 1, OwnerId = 100, Title = "A", Description = "desc A",
                City = "CityA", CategoryId = 10, Condition = ItemCondition.New,
                ItemTags = new List<ItemTag> { new() { Tag = new Tag { Name = "t1" } } }
            },
            new Item
            {
                Id = 2, OwnerId = 200, Title = "B", Description = null,
                City = null, CategoryId = null, Condition = null,
                ItemTags = new List<ItemTag>() 
            }
        };

        var pageFromRepo = new PagedResult<Item>(items, totalCount: 2, page: 1, pageSize: 20);

        var repo = new Mock<IItemRepository>();
        repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition, q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageFromRepo);

        var handler = new GetCatalogHandler(repo.Object);

        // Act
        var res = await handler.Handle(q, CancellationToken.None);

        // Assert 
        res.TotalCount.Should().Be(2);
        res.Page.Should().Be(1);
        res.PageSize.Should().Be(20);

        // Assert
        res.Items[0].Should().BeEquivalentTo(new CatalogItem(
            Id: 1, Title: "A", Description: "desc A", City: "CityA",
            CategoryId: 10, Condition: ItemCondition.New, Tags: new[] { "t1" }
        ));

        // Assert
        res.Items[1].Id.Should().Be(2);
        res.Items[1].Title.Should().Be("B");
        res.Items[1].Description.Should().BeNull();
        res.Items[1].City.Should().BeNull();
        res.Items[1].CategoryId.Should().BeNull();
        res.Items[1].Condition.Should().BeNull();
        res.Items[1].Tags.Should().NotBeNull().And.BeEmpty();

        // Verify
        repo.Verify(r => r.GetCatalogAsync(
            q.Page, q.PageSize, q.SortBy, q.SortDir,
            q.CategoryId, q.Condition, q.City, q.Search, q.Tags, true,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Маппинг и пагинация: все поля переносятся корректно, параметры пробрасываются в репозиторий")]
    public async Task Handle_MapsItemsAndPaging_AndForwardsAllParameters()
    {
        // Arrange 
        var q = new GetCatalogQuery(
            Page: 2,
            PageSize: 3,
            SortBy: "title",
            SortDir: "desc",
            CategoryId: 5,
            Condition: ItemCondition.Fair,
            City: "Berlin",
            Search: "bike",
            Tags: new[] { "mtb", "sport" },
            OnlyActive: true
        );

        var items = new List<Item>
        {
            MakeItem(10, "Bike", "MTB", "Berlin", 5, ItemCondition.Fair, "mtb", "sport"),
            MakeItem(11, "Helmet", "MIPS", "Berlin", 5, ItemCondition.New, "safety")
        };
        var pageFromRepo = new PagedResult<Item>(items, totalCount: 42, page: 2, pageSize: 3);

        _repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition,
                q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
             .ReturnsAsync(pageFromRepo);

        var handler = new GetCatalogHandler(_repo.Object);

        // Act
        var result = await handler.Handle(q, CancellationToken.None);

        // Assert 
        result.TotalCount.Should().Be(42);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(3);

        // Assert 
        result.Items.Should().HaveCount(2);

        result.Items[0].Should().BeEquivalentTo(new CatalogItem(
            Id: 10,
            Title: "Bike",
            Description: "MTB",
            City: "Berlin",
            CategoryId: 5,
            Condition: ItemCondition.Fair,
            Tags: new[] { "mtb", "sport" }
        ));

        result.Items[1].Should().BeEquivalentTo(new CatalogItem(
            Id: 11,
            Title: "Helmet",
            Description: "MIPS",
            City: "Berlin",
            CategoryId: 5,
            Condition: ItemCondition.New,
            Tags: new[] { "safety" }
        ));

        // Verify
        _repo.Verify(r => r.GetCatalogAsync(
            q.Page, q.PageSize, q.SortBy, q.SortDir,
            q.CategoryId, q.Condition,
            q.City, q.Search, q.Tags, q.OnlyActive,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Пустой результат: возвращаем пустой список и сохраняем параметры пагинации")]
    public async Task Handle_EmptyRepoResult_ReturnsEmptyItemsButKeepsPaging()
    {
        // Arrange
        var q = new GetCatalogQuery(1, 20, null, null, null, null, null, null, null, true);
        var empty = new PagedResult<Item>(new List<Item>(), totalCount: 0, page: 1, pageSize: 20);

        _repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition,
                q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
             .ReturnsAsync(empty);

        var handler = new GetCatalogHandler(_repo.Object);

        // Act
        var result = await handler.Handle(q, CancellationToken.None);

        // Assert
        result.Items.Should().NotBeNull().And.BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact(DisplayName = "Элемент без тегов: теги маппятся в пустой массив")]
    public async Task Handle_ItemWithoutTags_ProducesEmptyTagsArray()
    {
        // Arrange
        var q = new GetCatalogQuery(1, 10, null, null, null, null, null, null, null, true);

        var itemNoTags = MakeItem(7, "No tags", "desc", "City", null, null);
        itemNoTags.ItemTags.Clear();

        var page = new PagedResult<Item>(new List<Item> { itemNoTags }, totalCount: 1, page: 1, pageSize: 10);

        _repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition,
                q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
             .ReturnsAsync(page);

        var handler = new GetCatalogHandler(_repo.Object);

        // Act
        var result = await handler.Handle(q, CancellationToken.None);

        // Assert 
        result.Items.Should().HaveCount(1);
        var dto = result.Items[0];

        dto.Id.Should().Be(7);
        dto.Title.Should().Be("No tags");
        dto.Description.Should().Be("desc");
        dto.City.Should().Be("City");
        dto.CategoryId.Should().BeNull();
        dto.Condition.Should().BeNull();
        dto.Tags.Should().NotBeNull().And.BeEmpty();
    }

    [Fact(DisplayName = "Поле City/CategoryId/Condition: поддерживаются null и непустые значения")]
    public async Task Handle_MixesNullAndNonNull_MapsAllFields()
    {
        // Arrange
        var q = new GetCatalogQuery(1, 10, null, null, null, null, null, null, null, true);

        var items = new List<Item>
        {
            MakeItem(1, "T1", "D1", "Moscow", 100, ItemCondition.New, "a"),
            MakeItem(2, "T2", null, null, null, null /* no tags */)
        };
        items[1].ItemTags.Clear();

        var page = new PagedResult<Item>(items, totalCount: 2, page: 1, pageSize: 10);

        _repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition,
                q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
             .ReturnsAsync(page);

        var handler = new GetCatalogHandler(_repo.Object);

        // Act
        var res = await handler.Handle(q, CancellationToken.None);

        // Assert 
        res.Items[0].Should().BeEquivalentTo(new CatalogItem(
            Id: 1,
            Title: "T1",
            Description: "D1",
            City: "Moscow",
            CategoryId: 100,
            Condition: ItemCondition.New,
            Tags: new[] { "a" }
        ));

        // Assert 
        res.Items[1].Id.Should().Be(2);
        res.Items[1].Title.Should().Be("T2");
        res.Items[1].Description.Should().BeNull();
        res.Items[1].City.Should().BeNull();
        res.Items[1].CategoryId.Should().BeNull();
        res.Items[1].Condition.Should().BeNull();
        res.Items[1].Tags.Should().NotBeNull().And.BeEmpty();
    }

    [Fact(DisplayName = "Порядок тегов сохраняется таким, как вернул репозиторий")]
    public async Task Handle_PreservesTagsOrder()
    {
        // Arrange
        var q = new GetCatalogQuery(1, 5, null, null, null, null, null, null, null, true);

        var item = MakeItem(100, "T", "D", "City", 1, ItemCondition.Fair, "b", "a", "c");
        var page = new PagedResult<Item>(new List<Item> { item }, totalCount: 1, page: 1, pageSize: 5);

        _repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition,
                q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
             .ReturnsAsync(page);

        var handler = new GetCatalogHandler(_repo.Object);

        // Act
        var res = await handler.Handle(q, CancellationToken.None);

        // Assert
        res.Items.Should().ContainSingle();
        res.Items[0].Tags.Should().Equal("b", "a", "c");
    }

    [Fact(DisplayName = "OnlyActive=false: параметр пробрасывается в репозиторий как есть")]
    public async Task Handle_OnlyActiveFalse_IsForwardedAsIs()
    {
        // Arrange
        var q = new GetCatalogQuery(
            Page: 3,
            PageSize: 15,
            SortBy: null,
            SortDir: null,
            CategoryId: null,
            Condition: null,
            City: null,
            Search: "query",
            Tags: new[] { "x" },
            OnlyActive: false 
        );

        var page = new PagedResult<Item>(new List<Item>(), totalCount: 0, page: 3, pageSize: 15);

        _repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition,
                q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
             .ReturnsAsync(page);

        var handler = new GetCatalogHandler(_repo.Object);

        // Act
        _ = await handler.Handle(q, CancellationToken.None);

        // Assert 
        _repo.Verify(r => r.GetCatalogAsync(
            q.Page, q.PageSize, q.SortBy, q.SortDir,
            q.CategoryId, q.Condition,
            q.City, q.Search, q.Tags, false,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Некорректные данные из репозитория: ItemTag.Tag = null приводит к исключению (контракт маппера)")]
    public async Task Handle_NullTagInsideItemTag_Throws()
    {
        // Arrange
        var q = new GetCatalogQuery(1, 10, null, null, null, null, null, null, null, true);

        var broken = MakeItem(5, "Broken", "Desc", "City", 1, ItemCondition.Fair, "ok");
        broken.ItemTags.Add(new ItemTag { Tag = null! });

        var page = new PagedResult<Item>(new List<Item> { broken }, totalCount: 1, page: 1, pageSize: 10);

        _repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition,
                q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
             .ReturnsAsync(page);

        var handler = new GetCatalogHandler(_repo.Object);

        // Act
        var act = async () => await handler.Handle(q, CancellationToken.None);

        // Assert 
        await act.Should().ThrowAsync<NullReferenceException>();
    }
    
    [Fact(DisplayName = "Общий каталог: Tags = пустой массив — пробрасывается как есть, маппинг корректный")]
    public async Task Handle_GeneralCatalog_EmptyTagsArray_Forwarded()
    {
        // Arrange
        var q = new GetCatalogQuery(
            Page: 1, PageSize: 10,
            SortBy: null, SortDir: null,
            CategoryId: null, Condition: null,
            City: null, Search: null,
            Tags: System.Array.Empty<string>(),
            OnlyActive: true
        );

        var items = new List<Item>
        {
            new Item
            {
                Id = 7, OwnerId = 1, Title = "X", Description = "DX",
                City = "", CategoryId = null, Condition = ItemCondition.Fair,
                ItemTags = new List<ItemTag> { new() { Tag = new Tag { Name = "one" } } }
            }
        };

        var page = new PagedResult<Item>(items, totalCount: 1, page: 1, pageSize: 10);

        var repo = new Mock<IItemRepository>();
        repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition, q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var handler = new GetCatalogHandler(repo.Object);

        // Act
        var res = await handler.Handle(q, CancellationToken.None);

        // Assert 
        res.Items.Should().ContainSingle();
        res.Items[0].Should().BeEquivalentTo(new CatalogItem(
            Id: 7, Title: "X", Description: "DX", City: "",
            CategoryId: null, Condition: ItemCondition.Fair, Tags: new[] { "one" }
        ));

        // Verify 
        repo.Verify(r => r.GetCatalogAsync(
            1, 10, null, null, null, null, null, null,
            It.Is<string[]>(t => t.Length == 0),
            true,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Общий каталог: проброс CancellationToken без фильтров/сортировки")]
    public async Task Handle_GeneralCatalog_NoFilters_CancellationTokenForwarded()
    {
        // Arrange
        var q = new GetCatalogQuery(1, 5, null, null, null, null, null, null, null, true);

        var repo = new Mock<IItemRepository>();
        repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition, q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(new PagedResult<Item>(new List<Item>(), 0, 1, 5));

        var handler = new GetCatalogHandler(repo.Object);
        using var cts = new CancellationTokenSource();

        // Act
        _ = await handler.Handle(q, cts.Token);

        // Assert 
        repo.Verify(r => r.GetCatalogAsync(
            1, 5, null, null, null, null, null, null, null, true,
            It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
    }

    [Fact(DisplayName = "Общий каталог: репозиторий может вернуть сколько угодно элементов — хэндлер маппит всё как есть")]
    public async Task Handle_GeneralCatalog_MapsAllReturnedItems()
    {
        // Arrange
        var q = new GetCatalogQuery(1, 2, null, null, null, null, null, null, null, true);

        var items = Enumerable.Range(1, 5).Select(i => new Item
        {
            Id = i,
            OwnerId = 1,
            Title = $"T{i}",
            Description = i % 2 == 0 ? null : $"D{i}",
            City = $"C{i}",
            CategoryId = i % 2 == 0 ? null : i,
            Condition = i % 2 == 0 ? null : ItemCondition.Fair,
            ItemTags = new List<ItemTag> { new() { Tag = new Tag { Name = $"tag{i}" } } }
        }).ToList();

        var page = new PagedResult<Item>(items, totalCount: 5, page: 1, pageSize: 2);

        var repo = new Mock<IItemRepository>();
        repo.Setup(r => r.GetCatalogAsync(
                q.Page, q.PageSize, q.SortBy, q.SortDir,
                q.CategoryId, q.Condition, q.City, q.Search, q.Tags, q.OnlyActive,
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(page);

        var handler = new GetCatalogHandler(repo.Object);

        // Act
        var res = await handler.Handle(q, CancellationToken.None);

        // Assert 
        res.Items.Should().HaveCount(5);
        res.TotalCount.Should().Be(5);
        res.Page.Should().Be(1);
        res.PageSize.Should().Be(2);

        res.Items[1].Title.Should().Be("T2");
        res.Items[1].Description.Should().BeNull();
        res.Items[1].CategoryId.Should().BeNull();
        res.Items[1].Condition.Should().BeNull();
        res.Items[1].Tags.Should().Equal("tag2");
    }
}
