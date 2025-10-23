using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SwipeSwap.Application.Exchanges.Dtos;
using SwipeSwap.Application.Exchanges.Handlers;
using SwipeSwap.Domain.Models;
using SwipeSwap.Domain.Models.Enums;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;
using Xunit;

namespace SwipeSwap.UnitTests.HandlerTests.Exchanges;

public class AcceptExchangeHandlerTests
{
    private static Exchange MakeSentExchange(
        int id,
        int initiatorId,
        int receiverId,
        int offeredItemId = 1,
        int requestedItemId = 2,
        string? message = "hi")
    {
        var ex = Exchange.Create(initiatorId, receiverId, offeredItemId, requestedItemId, message);
        ex.Id = id;
        return ex;
    }

    [Fact(DisplayName = "Принятие обмена: успешный сценарий — статус меняется на Accepted, сохраняемся, DTO = Accepted")]
    public async Task Handle_Success_AcceptsAndPersists_AndReturnsDtoAccepted()
    {
        // Arrange
        var exchangeId = 55;
        var initiatorId = 10;
        var receiverId = 20;

        var entity = MakeSentExchange(exchangeId, initiatorId, receiverId);

        var repo = new Mock<IExchangeRepository>();
        repo.Setup(r => r.GetByIdAsync(exchangeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new AcceptExchangeHandler(repo.Object);
        var cmd = new AcceptExchangeRequest(exchangeId, receiverId);

        // Act
        var dto = await handler.Handle(cmd, CancellationToken.None);

        // Assert 
        entity.Status.Should().Be(ExchangeStatus.Accepted);
        entity.UpdatedAt.Should().NotBeNull();

        // Assert 
        dto.Should().NotBeNull();
        dto.Id.Should().Be(exchangeId);
        dto.Status.Should().Be(nameof(ExchangeStatus.Accepted));

        // Verify
        repo.Verify(r => r.GetByIdAsync(exchangeId, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Принятие обмена: обмен не найден — KeyNotFoundException, сохранения нет")]
    public async Task Handle_NotFound_Throws_KeyNotFound_NoSave()
    {
        // Arrange
        var repo = new Mock<IExchangeRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exchange?)null);

        var handler = new AcceptExchangeHandler(repo.Object);
        var cmd = new AcceptExchangeRequest(999, 1);

        // Act
        var act = async () => await handler.Handle(cmd, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Exchange not found");
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Принятие обмена: принять может только получатель — InvalidOperationException, сохранения нет")]
    public async Task Handle_ActorIsNotReceiver_Throws_InvalidOperation_NoSave()
    {
        // Arrange
        var ex = MakeSentExchange(id: 7, initiatorId: 1, receiverId: 2);
        var wrongActorId = 3; 

        var repo = new Mock<IExchangeRepository>();
        repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ex);

        var handler = new AcceptExchangeHandler(repo.Object);
        var cmd = new AcceptExchangeRequest(ex.Id, wrongActorId);

        // Act
        var act = async () => await handler.Handle(cmd, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Принять предложение может только получатель.");
        ex.Status.Should().Be(ExchangeStatus.Sent);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Принятие обмена: статус не Sent — InvalidOperationException, сохранения нет")]
    public async Task Handle_StatusIsNotSent_Throws_InvalidOperation_NoSave()
    {
        // Arrange
        var ex = MakeSentExchange(id: 8, initiatorId: 1, receiverId: 2);
        ex.Accept(actorUserId: 2);

        var repo = new Mock<IExchangeRepository>();
        repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ex);

        var handler = new AcceptExchangeHandler(repo.Object);
        var cmd = new AcceptExchangeRequest(ex.Id, ActorUserId: 2); 

        // Act
        var act = async () => await handler.Handle(cmd, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Можно принять только предложение в статусе Sent.");
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Принятие обмена: CancellationToken пробрасывается в репозиторий (GetByIdAsync и SaveChangesAsync)")]
    public async Task Handle_Forwards_CancellationToken()
    {
        // Arrange
        var ex = MakeSentExchange(id: 44, initiatorId: 5, receiverId: 6);

        using var cts = new CancellationTokenSource();

        var repo = new Mock<IExchangeRepository>();
        repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ex);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new AcceptExchangeHandler(repo.Object);
        var cmd = new AcceptExchangeRequest(ex.Id, ActorUserId: 6); 

        // Act
        _ = await handler.Handle(cmd, cts.Token);

        // Assert
        repo.Verify(r => r.GetByIdAsync(ex.Id, It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
    }
}
