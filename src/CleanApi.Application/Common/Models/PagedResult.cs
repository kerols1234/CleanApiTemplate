using System.Text.Json.Serialization;

namespace CleanApi.Application.Common.Models;

/// <summary>A page of results plus the metadata a client needs to render pagination.</summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }

    public int Page { get; }

    public int Size { get; }

    public int TotalCount { get; }

    public int TotalPages { get; }

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    [JsonConstructor]
    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int size)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        Size = size;
        TotalPages = size <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)size);
    }

    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int page, int size) =>
        new(items, totalCount, page, size);
}
