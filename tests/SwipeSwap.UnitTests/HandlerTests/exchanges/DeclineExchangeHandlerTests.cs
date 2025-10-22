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
    public class DeclineExchangeHandlerTests
    {
        private static Exchange MakeSentExchange(
            int id,
            int initiatorId,
            int receiverId,
            int offeredItemId = 1,
            int requestedItemId = 2,
            string? message = "hello")
        {
            var ex = Exchange.Create(initiatorId, receiverId, offeredItemId, requestedItemId, message);
            ex.Id = id;
            return ex;
        }

        [Fact(DisplayName = "Отклонение обмена: успешный сценарий — статус Declined, SaveChanges вызван, причина добавлена в Message")]
        public async Task Handle_Success_DeclinesAndPersists_AppendsReason_ReturnsDtoDeclined()
        {
            // Arrange
            var ex = MakeSentExchange(id: 301, initiatorId: 10, receiverId: 20, message: "base");
            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);
            repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new DeclineExchangeHandler(repo.Object);
            var cmd = new DeclineExchangeRequest(ExchangeId: ex.Id, ActorUserId: 20, Reason: "не подходит");

            // Act
            var dto = await handler.Handle(cmd, CancellationToken.None);

            // Assert 
            ex.Status.Should().Be(ExchangeStatus.Declined);
            ex.UpdatedAt.Should().NotBeNull();
            ex.Message.Should().Contain("[Decline]: не подходит");

            // Assert 
            dto.Should().NotBeNull();
            dto.Id.Should().Be(ex.Id);
            dto.Status.ToString().Should().Be(nameof(ExchangeStatus.Declined));

            // Verify
            repo.Verify(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>()), Times.Once);
            repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Отклонение обмена: NotFound → KeyNotFoundException, сохранения нет")]
        public async Task Handle_NotFound_Throws_KeyNotFound_NoSave()
        {
            // Arrange
            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Exchange?)null);

            var handler = new DeclineExchangeHandler(repo.Object);
            var cmd = new DeclineExchangeRequest(999, 1, "reason");

            // Act
            var act = async () => await handler.Handle(cmd, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Exchange not found");
            repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Отклонение обмена: отклонять может только получатель — InvalidOperationException, статус не меняется")]
        public async Task Handle_ActorIsNotReceiver_Throws_InvalidOperation_NoSave()
        {
            // Arrange
            var ex = MakeSentExchange(id: 302, initiatorId: 1, receiverId: 2);
            var notReceiver = 3;

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);

            var handler = new DeclineExchangeHandler(repo.Object);
            var cmd = new DeclineExchangeRequest(ex.Id, notReceiver, "nope");

            // Act
            var act = async () => await handler.Handle(cmd, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Отклонить предложение может только получатель.");
            ex.Status.Should().Be(ExchangeStatus.Sent);
            repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Отклонение обмена: можно только из статуса Sent — иначе InvalidOperationException")]
        public async Task Handle_StatusIsNotSent_Throws_InvalidOperation_NoSave()
        {
            // Arrange 
            var ex = MakeSentExchange(id: 303, initiatorId: 11, receiverId: 22);
            ex.Accept(actorUserId: 22);

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);

            var handler = new DeclineExchangeHandler(repo.Object);
            var cmd = new DeclineExchangeRequest(ex.Id, 22, "late");

            // Act
            var act = async () => await handler.Handle(cmd, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Отклонить можно только предложение в статусе Sent.");
            ex.Status.Should().Be(ExchangeStatus.Accepted);
            repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Отклонение обмена: пустая/пробельная причина — Message не дополняется")]
        public async Task Handle_EmptyReason_DoesNotAppendMessage()
        {
            // Arrange
            var ex = MakeSentExchange(id: 304, initiatorId: 5, receiverId: 6, message: "orig");

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);
            repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new DeclineExchangeHandler(repo.Object);

            // Act
            _ = await handler.Handle(new DeclineExchangeRequest(ex.Id, 6, "   "), CancellationToken.None);

            // Assert
            ex.Status.Should().Be(ExchangeStatus.Declined);
            ex.Message.Should().Be("orig"); 
        }

        [Fact(DisplayName = "Отклонение обмена: проброс CancellationToken в GetByIdAsync и SaveChangesAsync")]
        public async Task Handle_Forwards_CancellationToken()
        {
            // Arrange
            var ex = MakeSentExchange(id: 305, initiatorId: 7, receiverId: 8);

            using var cts = new CancellationTokenSource();

            var repo = new Mock<IExchangeRepository>();
            repo.Setup(r => r.GetByIdAsync(ex.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ex);
            repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new DeclineExchangeHandler(repo.Object);

            // Act
            _ = await handler.Handle(new DeclineExchangeRequest(ex.Id, 8, "reason"), cts.Token);

            // Assert
            repo.Verify(r => r.GetByIdAsync(ex.Id, It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
            repo.Verify(r => r.SaveChangesAsync(It.Is<CancellationToken>(t => t.Equals(cts.Token))), Times.Once);
        }
    }
}
