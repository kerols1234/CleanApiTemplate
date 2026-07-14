namespace CleanApi.Application.Common.Models;

/// <summary>Something that can be sorted dynamically by a field name + direction.</summary>
public interface ISortable
{
    string? SortField { get; }
    string? SortOrder { get; }
}

/// <summary>
/// Base for paged list queries: page/size + optional free-text search and dynamic sort.
/// <see cref="Page"/> and <see cref="Size"/> are clamped so bad input can't crash paging.
/// </summary>
public abstract record PagedRequest : ISortable
{
    private const int MaxSize = 100;
    private int _page = 1;
    private int _size = 10;

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    public int Size
    {
        get => _size;
        init => _size = value is < 1 or > MaxSize ? Math.Clamp(value, 1, MaxSize) : value;
    }

    public string? Search { get; init; }

    public string? SortField { get; init; }

    /// <summary>"asc" (default) or "desc".</summary>
    public string? SortOrder { get; init; } = "asc";
}
