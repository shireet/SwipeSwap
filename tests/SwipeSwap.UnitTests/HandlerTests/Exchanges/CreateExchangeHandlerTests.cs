using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using SwipeSwap.Application.Common.Exceptions;
using SwipeSwap.Application.Exchanges;
using SwipeSwap.Application.Exchanges.Dtos;
using SwipeSwap.Application.Exchanges.Handlers;
using SwipeSwap.Domain.Models;
using SwipeSwap.Domain.Models.Enums;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;
using Xunit;

namespace SwipeSwap.UnitTests.HandlerTests.Exchanges;

public class CreateExchangeHandlerTests
{
    private static Item MakeItem(int id, int ownerId, bool isActive = true)
        => new Item
        {
            Id = id,
            OwnerId = ownerId,
            Title = $"Item {id}",
            IsActive = isActive,
            ItemTags = new List<ItemTag>()
        };

    [Fact(DisplayName = "Создание обмена: успех — создаём, сохраняем, DTO корректен")]
    public async Task Handle_Success_CreatesAndPersists_ReturnsDto()
    {
        // Arrange
        var me = 10;
        var offeredId = 101;
        var requestedId = 202;
        var otherUser = 20;
        var message = "Привет! Поменяемся?";

        var offered = MakeItem(offeredId, me, isActive: true);
        var requested = MakeItem(requestedId, otherUser, isActive: true);

        var items = new Mock<IItemRepository>();
        items.Setup(r => r.GetByIdAsync(offeredId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(offered);
        items.Setup(r => r.GetByIdAsync(requestedId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(requested);

        Exchange? captured = null;
        var exchanges = new Mock<IExchangeRepository>();
        exchanges.Setup(r => r.ExistsOpenForPairAsync(me, offeredId, requestedId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);
        exchanges.Setup(r => r.AddAsync(It.IsAny<Exchange>(), It.IsAny<CancellationToken>()))
                 .Callback<Exchange, CancellationToken>((e, _) =>
                 {
                     captured = e;
                     e.Id = 555;
                 })
                 .Returns(Task.CompletedTask);
        exchanges.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var handler = new CreateExchangeHandler(items.Object, exchanges.Object);

        var req = new CreateExchangeRequest(
            InitiatorUserId: me,
            OfferedItemId: offeredId,
            RequestedItemId: requestedId,
            Message: message
        );

        // Act
        var dto = await handler.Handle(req, CancellationToken.None);

        // Assert 
        captured.Should().NotBeNull();
        captured!.InitiatorId.Should().Be(me);
        captured.ReceiverId.Should().Be(otherUser);
        captured.OfferedItemId.Should().Be(offeredId);
        captured.RequestedItemId.Should().Be(requestedId);
        captured.Message.Should().Be(message);
        captured.Status.Should().Be(ExchangeStatus.Sent);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(555);
        dto.Status.ToString().Should().Be(nameof(ExchangeStatus.Sent));
        dto.InitiatorId.Should().Be(me);
        dto.ReceiverId.Should().Be(otherUser);
        dto.OfferedItemId.Should().Be(offeredId);
        dto.RequestedItemId.Should().Be(requestedId);

        // Verify 
        items.Verify(r => r.GetByIdAsync(offeredId, It.IsAny<CancellationToken>()), Times.Once);
        items.Verify(r => r.GetByIdAsync(requestedId, It.IsAny<CancellationToken>()), Times.Once);
        exchanges.Verify(r => r.ExistsOpenForPairAsync(me, offeredId, requestedId, It.IsAny<CancellationToken>()), Times.Once);
        exchanges.Verify(r => r.AddAsync(It.IsAny<Exchange>(), It.IsAny<CancellationToken>()), Times.Once);
        exchanges.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Создание обмена: предмет для предложения не найден — NotFoundException, ничего не сохраняем")]
    public async Task Handle_OfferedNotFound_ThrowsNotFound_NoSave()
    {
        // Arrange
        var items = new Mock<IItemRepository>();
        items.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Item?)null);

        var exchanges = new Mock<IExchangeRepository>();

        var handler = new CreateExchangeHandler(items.Object, exchanges.Object);
        var req = new CreateExchangeRequest(10, 1, 2, null);

        // Act
        var act = async () => await handler.Handle(req, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Предмет для предложения с ID 1 не найден.");
        exchanges.Verify(r => r.AddAsync(It.IsAny<Exchange>(), It.IsAny<CancellationToken>()), Times.Never);
        exchanges.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Создание обмена: запрашиваемый предмет не найден — NotFoundException, ничего не сохраняем")]
    public async Task Handle_RequestedNotFound_ThrowsNotFound_NoSave()
    {
        // Arrange
        var items = new Mock<IItemRepository>();
        items.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(1, 10, true));
        items.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Item?)null);

        var exchanges = new Mock<IExchangeRepository>();

        var handler = new CreateExchangeHandler(items.Object, exchanges.Object);
        var req = new CreateExchangeRequest(10, 1, 2, null);

        // Act
        var act = async () => await handler.Handle(req, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Запрашиваемый предмет с ID 2 не найден.");
        exchanges.Verify(r => r.AddAsync(It.IsAny<Exchange>(), It.IsAny<CancellationToken>()), Times.Never);
        exchanges.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Создание обмена: предлагаемый предмет не принадлежит инициатору — ForbiddenException")]
    public async Task Handle_OfferedNotOwned_ThrowsForbidden()
    {
        // Arrange
        var me = 10;
        var items = new Mock<IItemRepository>();
        items.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(1, ownerId: 999, isActive: true)); 
        items.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(2, ownerId: 20, isActive: true));

        var exchanges = new Mock<IExchangeRepository>();
        var handler = new CreateExchangeHandler(items.Object, exchanges.Object);
        var req = new CreateExchangeRequest(me, 1, 2, null);

        // Act
        var act = async () => await handler.Handle(req, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Вы можете предлагать только те предметы, которыми владеете.");
        exchanges.Verify(r => r.AddAsync(It.IsAny<Exchange>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Создание обмена: нельзя обменяться с самим собой — ValidationException")]
    public async Task Handle_SelfExchange_ThrowsValidation()
    {
        // Arrange
        var me = 10;
        var items = new Mock<IItemRepository>();
        items.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(1, ownerId: me, isActive: true));
        items.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(2, ownerId: me, isActive: true));

        var exchanges = new Mock<IExchangeRepository>();
        var handler = new CreateExchangeHandler(items.Object, exchanges.Object);
        var req = new CreateExchangeRequest(me, 1, 2, null);

        // Act
        var act = async () => await handler.Handle(req, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Нельзя отправлять обмен самому себе.");
    }

    [Fact(DisplayName = "Создание обмена: предлагаемый и запрашиваемый предметы одинаковые — ValidationException")]
    public async Task Handle_SameItemIds_ThrowsValidation()
    {
        // Arrange
        var me = 10;

        var offered = new Item
        {
            Id = 1,
            OwnerId = me,
            Title = "Item 1",
            IsActive = true,
            ItemTags = new List<ItemTag>()
        };
        var requested = new Item
        {
            Id = 1,
            OwnerId = 999,
            Title = "Item 1",
            IsActive = true,
            ItemTags = new List<ItemTag>()
        };

        var items = new Mock<IItemRepository>();
        items.SetupSequence(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(offered)   
            .ReturnsAsync(requested); 

        var exchanges = new Mock<IExchangeRepository>();
        var handler = new CreateExchangeHandler(items.Object, exchanges.Object);
        var req = new CreateExchangeRequest(me, 1, 1, null);

        // Act
        var act = async () => await handler.Handle(req, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Предлагаемый и запрашиваемый предметы должны быть разными.");
        exchanges.Verify(r => r.AddAsync(It.IsAny<Exchange>(), It.IsAny<CancellationToken>()), Times.Never);
        exchanges.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Создание обмена: предлагаемый предмет неактивен — ValidationException")]
    public async Task Handle_OfferedInactive_ThrowsValidation()
    {
        // Arrange
        var me = 10;
        var items = new Mock<IItemRepository>();
        items.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(1, me, isActive: false));
        items.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(2, 20, isActive: true));

        var exchanges = new Mock<IExchangeRepository>();
        var handler = new CreateExchangeHandler(items.Object, exchanges.Object);
        var req = new CreateExchangeRequest(me, 1, 2, null);

        // Act
        var act = async () => await handler.Handle(req, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Ваш предмет недоступен для обмена.");
    }

    [Fact(DisplayName = "Создание обмена: запрашиваемый предмет неактивен — ValidationException")]
    public async Task Handle_RequestedInactive_ThrowsValidation()
    {
        // Arrange
        var me = 10;
        var items = new Mock<IItemRepository>();
        items.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(1, me, isActive: true));
        items.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(2, 20, isActive: false));

        var exchanges = new Mock<IExchangeRepository>();
        var handler = new CreateExchangeHandler(items.Object, exchanges.Object);
        var req = new CreateExchangeRequest(me, 1, 2, null);

        // Act
        var act = async () => await handler.Handle(req, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Запрашиваемый предмет недоступен для обмена.");
    }

    [Fact(DisplayName = "Создание обмена: активный оффер по паре уже существует — ValidationException")]
    public async Task Handle_OpenOfferAlreadyExists_ThrowsValidation()
    {
        // Arrange
        var me = 10;
        var offeredId = 1;
        var requestedId = 2;

        var items = new Mock<IItemRepository>();
        items.Setup(r => r.GetByIdAsync(offeredId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(offeredId, me, isActive: true));
        items.Setup(r => r.GetByIdAsync(requestedId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(requestedId, 20, isActive: true));

        var exchanges = new Mock<IExchangeRepository>();
        exchanges.Setup(r => r.ExistsOpenForPairAsync(me, offeredId, requestedId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        var handler = new CreateExchangeHandler(items.Object, exchanges.Object);
        var req = new CreateExchangeRequest(me, offeredId, requestedId, null);

        // Act
        var act = async () => await handler.Handle(req, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Вы уже отправили активный оффер для этой пары предметов.");
        exchanges.Verify(r => r.AddAsync(It.IsAny<Exchange>(), It.IsAny<CancellationToken>()), Times.Never);
        exchanges.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Создание обмена: CancellationToken пробрасывается во все репозитории")]
    public async Task Handle_Forwards_CancellationToken()
    {
        // Arrange
        var me = 10;
        var offeredId = 1;
        var requestedId = 2;

        var items = new Mock<IItemRepository>();
        items.Setup(r => r.GetByIdAsync(offeredId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(offeredId, me, isActive: true));
        items.Setup(r => r.GetByIdAsync(requestedId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeItem(requestedId, 20, isActive: true));

        var exchanges = new Mock<IExchangeRepository>();
        exchanges.Setup(r => r.ExistsOpenForPairAsync(me, offeredId, requestedId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);
        exchanges.Setup(r => r.AddAsync(It.IsAny<Exchange>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
        exchanges.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        using var cts = new CancellationTokenSource();

        var handler = new CreateExchangeHandler(items.Object, exchanges.Object);
        var req = new CreateExchangeRequest(me, offeredId, requestedId, "msg");

        // Act
        _ = await handler.Handle(req, cts.Token);

        // Assert
        items.Verify(r => r.GetByIdAsync(offeredId, It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
        items.Verify(r => r.GetByIdAsync(requestedId, It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
        exchanges.Verify(r => r.ExistsOpenForPairAsync(me, offeredId, requestedId, It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
        exchanges.Verify(r => r.AddAsync(It.IsAny<Exchange>(), It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
        exchanges.Verify(r => r.SaveChangesAsync(It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
    }
}
