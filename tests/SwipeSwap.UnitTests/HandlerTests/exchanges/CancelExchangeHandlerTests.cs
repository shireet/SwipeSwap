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
    public class CancelExchangeHandlerTests
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

        [Fact(DisplayName = "Отмена обмена: успешная отмена инициатором — статус Cancelled, SaveChanges вызван, DTO=Cancelled")]
        public async Task Handle_Success_ByInitiator_CancelsAndPersists_ReturnsDtoCancelled()
        {
            // Arrange
            var ex = MakeSentExchange(id: 101, initiatorId: 10, receiverId: 20, message: "hello");
            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);
            repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new CancelExchangeHandler(repo.Object);
            var cmd = new CancelExchangeRequest(ExchangeId: ex.Id, ActorUserId: 10, Reason: "передумал");

            // Act
            var dto = await handler.Handle(cmd, CancellationToken.None);

            // Assert 
            ex.Status.Should().Be(ExchangeStatus.Cancelled);
            ex.UpdatedAt.Should().NotBeNull();
            ex.Message.Should().Contain("[Cancel]: передумал");

            // Assert 
            dto.Id.Should().Be(ex.Id);
            dto.Status.Should().Be(nameof(ExchangeStatus.Cancelled));

            // Verify
            repo.Verify(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>()), Times.Once);
            repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Отмена обмена: успешная отмена получателем — статус Cancelled")]
        public async Task Handle_Success_ByReceiver_Cancels()
        {
            // Arrange
            var ex = MakeSentExchange(id: 102, initiatorId: 11, receiverId: 21);
            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);
            repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new CancelExchangeHandler(repo.Object);
            var cmd = new CancelExchangeRequest(ex.Id, ActorUserId: 21, Reason: null);

            // Act
            var dto = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            ex.Status.Should().Be(ExchangeStatus.Cancelled);
            dto.Status.Should().Be(nameof(ExchangeStatus.Cancelled));
        }

        [Fact(DisplayName = "Отмена обмена: NotFound → KeyNotFoundException, SaveChanges не вызывается")]
        public async Task Handle_NotFound_Throws_KeyNotFound_NoSave()
        {
            // Arrange
            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Exchange?)null);

            var handler = new CancelExchangeHandler(repo.Object);

            // Act
            var act = async () => await handler.Handle(
                new CancelExchangeRequest(999, 1, "reason"),
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Exchange not found");
            repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Отмена обмена: актёр не участник — InvalidOperationException, статус не меняется, SaveChanges нет")]
        public async Task Handle_ActorNotParticipant_Throws_InvalidOperation_NoSave()
        {
            // Arrange
            var ex = MakeSentExchange(id: 103, initiatorId: 12, receiverId: 22);
            var strangerId = 999;

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);

            var handler = new CancelExchangeHandler(repo.Object);

            // Act
            var act = async () => await handler.Handle(
                new CancelExchangeRequest(ex.Id, ActorUserId: strangerId, Reason: "nope"),
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Отменить может только участник обмена.");
            ex.Status.Should().Be(ExchangeStatus.Sent);
            repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Отмена обмена: можно из статуса Accepted, статус становится Cancelled")]
        public async Task Handle_FromAccepted_CancelsOk()
        {
            // Arrange
            var ex = MakeSentExchange(id: 104, initiatorId: 13, receiverId: 23);
            ex.Accept(actorUserId: 23);

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);
            repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new CancelExchangeHandler(repo.Object);

            // Act 
            var dto = await handler.Handle(
                new CancelExchangeRequest(ex.Id, ActorUserId: 13, Reason: "не актуально"),
                CancellationToken.None);

            // Assert
            ex.Status.Should().Be(ExchangeStatus.Cancelled);
            dto.Status.Should().Be(nameof(ExchangeStatus.Cancelled));
            ex.Message.Should().Contain("[Cancel]: не актуально");
        }

        [Fact(DisplayName = "Отмена обмена: из статуса Completed нельзя — InvalidOperationException, SaveChanges нет")]
        public async Task Handle_FromCompleted_Throws_InvalidOperation()
        {
            // Arrange 
            var ex = MakeSentExchange(id: 105, initiatorId: 14, receiverId: 24);
            ex.Accept(actorUserId: 24);
            ex.Complete(actorUserId: 14);

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);

            var handler = new CancelExchangeHandler(repo.Object);

            // Act
            var act = async () => await handler.Handle(
                new CancelExchangeRequest(ex.Id, ActorUserId: 14, Reason: "слишком поздно"),
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Отменить можно только предложение в статусе Sent или Accepted.");
            repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Отмена обмена: если Reason пустой/пробельный — Message не дополняется")]
        public async Task Handle_EmptyReason_DoesNotAppendMessage()
        {
            // Arrange
            var ex = MakeSentExchange(id: 106, initiatorId: 15, receiverId: 25, message: "base");
            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);
            repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new CancelExchangeHandler(repo.Object);

            // Act
            _ = await handler.Handle(
                new CancelExchangeRequest(ex.Id, ActorUserId: 25, Reason: "   "),
                CancellationToken.None);

            // Assert
            ex.Status.Should().Be(ExchangeStatus.Cancelled);
            ex.Message.Should().Be("base"); 
        }

        [Fact(DisplayName = "Отмена обмена: проброс CancellationToken в GetByIdAsync и SaveChangesAsync")]
        public async Task Handle_Forwards_CancellationToken()
        {
            // Arrange
            var ex = MakeSentExchange(id: 107, initiatorId: 16, receiverId: 26);
            using var cts = new CancellationTokenSource();

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);
            repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new CancelExchangeHandler(repo.Object);

            // Act
            _ = await handler.Handle(new CancelExchangeRequest(ex.Id, ActorUserId: 16, Reason: null), cts.Token);

            // Assert
            repo.Verify(r => r.GetByIdAsync(ex.Id, It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
            repo.Verify(r => r.SaveChangesAsync(It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
        }
    }
}
