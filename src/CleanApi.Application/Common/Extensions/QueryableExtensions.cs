using System.Linq.Expressions;
using CleanApi.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using DynamicQueryable = System.Linq.Dynamic.Core.DynamicQueryableExtensions;

namespace CleanApi.Application.Common.Extensions;

/// <summary>Reusable <see cref="IQueryable{T}"/> helpers for filtering, dynamic sorting, and paging.</summary>
public static class QueryableExtensions
{
    /// <summary>Applies <paramref name="predicate"/> only when <paramref name="condition"/> is true.</summary>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate) =>
        condition ? query.Where(predicate) : query;

    /// <summary>
    /// Orders by an arbitrary property name at runtime (System.Linq.Dynamic.Core). Falls back to
    /// the source order when no sort field is supplied. Restrict allowed fields in the validator.
    /// </summary>
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, ISortable sortable)
    {
        if (string.IsNullOrWhiteSpace(sortable.SortField))
        {
            return query;
        }

        var descending = string.Equals(sortable.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return DynamicQueryable.OrderBy(query, $"{sortable.SortField} {(descending ? "descending" : "ascending")}");
    }

    /// <summary>Materializes a single page (with the total count) from the query.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(items, totalCount, page, size);
    }
}
