using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SwipeSwap.Application.Exchanges.Dtos;
using SwipeSwap.Application.Exchanges.Handlers;
using SwipeSwap.Domain.Models;
using SwipeSwap.Domain.Models.Enums;
using SwipeSwap.Infrastructure.Repositories.Interfaces;
using Xunit;

namespace SwipeSwap.UnitTests.HandlerTests.Exchanges
{
    public class CompleteExchangeHandlerTests
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

        [Fact(DisplayName = "Завершение обмена: успешный сценарий — статус Completed, SaveChanges вызван, DTO со статусом Completed")]
        public async Task Handle_Success_CompletesAndPersists_ReturnsDtoCompleted()
        {
            // Arrange
            var ex = MakeSentExchange(id: 201, initiatorId: 10, receiverId: 20, message: "msg");
            ex.Accept(actorUserId: 20);

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);
            repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new CompleteExchangeHandler(repo.Object);
            var cmd = new CompleteExchangeRequest(ExchangeId: ex.Id, ActorUserId: 10, Note: "всё ок");

            // Act
            var dto = await handler.Handle(cmd, CancellationToken.None);

            // Assert 
            ex.Status.Should().Be(ExchangeStatus.Completed);
            ex.UpdatedAt.Should().NotBeNull();
            ex.Message.Should().Contain("[Complete]: всё ок");

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(ex.Id);
            dto.Status.ToString().Should().Be(nameof(ExchangeStatus.Completed));

            // Verify
            repo.Verify(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>()), Times.Once);
            repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Завершение обмена: NotFound → KeyNotFoundException, SaveChanges не вызывается")]
        public async Task Handle_NotFound_Throws_KeyNotFound_NoSave()
        {
            // Arrange
            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Exchange?)null);

            var handler = new CompleteExchangeHandler(repo.Object);
            var cmd = new CompleteExchangeRequest(999, 1, "note");

            // Act
            var act = async () => await handler.Handle(cmd, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Exchange not found");
            repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Завершение обмена: завершать может только инициатор — InvalidOperationException, статус не меняется")]
        public async Task Handle_ActorIsNotInitiator_Throws_InvalidOperation_NoSave()
        {
            // Arrange
            var ex = MakeSentExchange(id: 202, initiatorId: 11, receiverId: 21);
            ex.Accept(actorUserId: 21);
            var notInitiator = 21; 

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);

            var handler = new CompleteExchangeHandler(repo.Object);
            var cmd = new CompleteExchangeRequest(ex.Id, notInitiator, "try");

            // Act
            var act = async () => await handler.Handle(cmd, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Завершить обмен может только инициатор (по умолчанию).");
            ex.Status.Should().Be(ExchangeStatus.Accepted);
            repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Завершение обмена: можно только из Accepted — иначе InvalidOperationException")]
        public async Task Handle_StatusIsNotAccepted_Throws_InvalidOperation_NoSave()
        {
            // Arrange
            var ex = MakeSentExchange(id: 203, initiatorId: 12, receiverId: 22);

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);

            var handler = new CompleteExchangeHandler(repo.Object);
            var cmd = new CompleteExchangeRequest(ex.Id, ActorUserId: 12, Note: null);

            // Act
            var act = async () => await handler.Handle(cmd, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Завершить можно только принятый обмен.");
            ex.Status.Should().Be(ExchangeStatus.Sent);
            repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Завершение обмена: пустая/пробельная Note — Message не дополняется")]
        public async Task Handle_EmptyNote_DoesNotAppendMessage()
        {
            // Arrange
            var ex = MakeSentExchange(id: 204, initiatorId: 13, receiverId: 23, message: "base");
            ex.Accept(actorUserId: 23);

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);
            repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new CompleteExchangeHandler(repo.Object);

            // Act
            _ = await handler.Handle(new CompleteExchangeRequest(ex.Id, 13, "   "), CancellationToken.None);

            // Assert
            ex.Status.Should().Be(ExchangeStatus.Completed);
            ex.Message.Should().Be("base"); 
        }

        [Fact(DisplayName = "Завершение обмена: проброс CancellationToken в GetByIdAsync и SaveChangesAsync")]
        public async Task Handle_Forwards_CancellationToken()
        {
            // Arrange
            var ex = MakeSentExchange(id: 205, initiatorId: 14, receiverId: 24);
            ex.Accept(actorUserId: 24);

            using var cts = new CancellationTokenSource();

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);
            repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new CompleteExchangeHandler(repo.Object);

            // Act
            _ = await handler.Handle(new CompleteExchangeRequest(ex.Id, 14, null), cts.Token);

            // Assert
            repo.Verify(r => r.GetByIdAsync(ex.Id, It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
            repo.Verify(r => r.SaveChangesAsync(It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
        }
    }
}
