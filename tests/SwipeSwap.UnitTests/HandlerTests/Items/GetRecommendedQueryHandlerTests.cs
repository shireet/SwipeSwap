using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SwipeSwap.Application.Items;
using SwipeSwap.Application.Items.Dtos;
using SwipeSwap.Application.Items.Handlers;
using SwipeSwap.Domain.Models;
using SwipeSwap.Domain.Models.Enums;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;
using Xunit;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace SwipeSwap.UnitTests.HandlerTests.Items
{
    using Microsoft.EntityFrameworkCore.Query;
    using System.Linq.Expressions;

    public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
            => new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new TestAsyncEnumerable<TElement>(expression);

        public object? Execute(Expression expression)
            => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            => _inner.Execute<TResult>(expression);

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            => new TestAsyncEnumerable<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            => Execute<TResult>(expression);
    }
    
    public static class EfCoreMockExtensions
    {
        public static IQueryable<T> AsNoTracking<T>(this IQueryable<T> source) => source;
    }

    public static class AsyncQueryableMock
    {
        public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
            => new TestAsyncEnumerable<T>(source);
    }
    
    public static class AsyncLinqExtensions
    {
        public static IQueryable<T> ToAsyncQueryable<T>(this IEnumerable<T> source)
            => new TestAsyncEnumerable<T>(source);

        public static IQueryable<T> ToAsyncQueryable<T>(this IQueryable<T> source)
            => new TestAsyncEnumerable<T>(source);
    }

    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
        public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
        public T Current => _inner.Current;
    }

    public class GetRecommendedQueryHandlerTests
    {
        [Fact]
        public async Task Returns_Items_With_Higher_Score_First()
        {
            var repoMock = new Mock<IItemRepository>();
            var userId = 1;

            var userItems = new List<Item>
            {
                new()
                {
                    Id = 10,
                    OwnerId = userId,
                    IsActive = true,
                    Title = "User item",
                    City = "Moscow",
                    CategoryId = 1,
                    ItemTags = new List<ItemTag> { new() { Tag = new Tag { Name = "music" } } }
                }
            };

            var otherItems = new List<Item>
            {
                new()
                {
                    Id = 1,
                    OwnerId = 2,
                    IsActive = true,
                    Title = "Same City and Category",
                    City = "Moscow",
                    CategoryId = 1,
                    ItemTags = new List<ItemTag> { new() { Tag = new Tag { Name = "music" } } }
                },
                new()
                {
                    Id = 2,
                    OwnerId = 2,
                    IsActive = true,
                    Title = "Different City and Category",
                    City = "SPB",
                    CategoryId = 3,
                    ItemTags = new List<ItemTag> { new() { Tag = new Tag { Name = "sport" } } }
                }
            };

            var allItems = userItems.Concat(otherItems).AsAsyncQueryable();
            repoMock.Setup(r => r.Items).Returns(allItems.ToAsyncQueryable());

            var handler = new GetRecommendedHandler(repoMock.Object);

            var result = await handler.Handle(new GetRecommendedQuery(userId, 1, 10), CancellationToken.None);

            result.Should().NotBeEmpty();
            result.First().Title.Should().Be("Same City and Category");
        }

        [Fact]
        public async Task Returns_Fallback_When_No_Similar_Items()
        {
            var repoMock = new Mock<IItemRepository>();
            var userId = 1;

            var userItems = new List<Item>
            {
                new()
                {
                    Id = 100,
                    OwnerId = userId,
                    IsActive = true,
                    Title = "User art item",
                    City = "Paris",
                    CategoryId = 99,
                    ItemTags = new List<ItemTag> { new() { Tag = new Tag { Name = "art" } } }
                }
            };

            var otherItems = new List<Item>
            {
                new()
                {
                    Id = 5,
                    OwnerId = 2,
                    IsActive = true,
                    Title = "Completely Different",
                    City = "Berlin",
                    CategoryId = 2,
                    ItemTags = new List<ItemTag> { new() { Tag = new Tag { Name = "sport" } } }
                }
            };

            var allItems = userItems.Concat(otherItems).AsAsyncQueryable();
            repoMock.Setup(r => r.Items).Returns(allItems.ToAsyncQueryable());

            var handler = new GetRecommendedHandler(repoMock.Object);

            var result = await handler.Handle(new GetRecommendedQuery(userId, 1, 10), CancellationToken.None);

            result.Should().ContainSingle();
            result.First().Title.Should().Be("Completely Different");
        }

        [Fact]
        public async Task Does_Not_Include_User_Items_In_Result()
        {
            var repoMock = new Mock<IItemRepository>();
            var userId = 1;

            var userItems = new List<Item>
            {
                new()
                {
                    Id = 10,
                    OwnerId = userId,
                    IsActive = true,
                    Title = "User Item (should not appear)",
                    City = "Moscow",
                    CategoryId = 1
                }
            };

            var otherItems = new List<Item>
            {
                new()
                {
                    Id = 11,
                    OwnerId = 2,
                    IsActive = true,
                    Title = "Recommended",
                    City = "Moscow",
                    CategoryId = 1
                },
                new()
                {
                    Id = 12,
                    OwnerId = userId,
                    IsActive = true,
                    Title = "User Item Duplicate",
                    City = "Moscow",
                    CategoryId = 1
                }
            };

            var allItems = userItems.Concat(otherItems).AsAsyncQueryable();
            repoMock.Setup(r => r.Items).Returns(allItems.ToAsyncQueryable());

            var handler = new GetRecommendedHandler(repoMock.Object);

            var result = await handler.Handle(new GetRecommendedQuery(userId, 1, 10), CancellationToken.None);

            result.Should().OnlyContain(i => i.Title != "User Item (should not appear)");
        }
    }
}
