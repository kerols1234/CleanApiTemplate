namespace CleanApi.Application.Common.Caching;

/// <summary>
/// Marks a query whose result should be cached by <c>CachingBehavior</c>. The cache is backed by
/// <c>HybridCache</c> (in-memory L1, plus Redis L2 when configured).
/// </summary>
public interface ICacheableQuery
{
    /// <summary>Stable, unique key for this specific query instance (include its parameters).</summary>
    string CacheKey { get; }

    /// <summary>Absolute expiration; null uses the configured default.</summary>
    TimeSpan? Expiration => null;

    /// <summary>Tags for group invalidation via <c>HybridCache.RemoveByTagAsync</c>.</summary>
    IReadOnlyCollection<string>? Tags => null;
}
