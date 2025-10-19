// src/SwipeSwap.Domain/Shared/PagedResult.cs
namespace SwipeSwap.Domain.Shared;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page < 1 ? 1 : page;
        PageSize = pageSize < 1 ? 1 : pageSize;
    }
}