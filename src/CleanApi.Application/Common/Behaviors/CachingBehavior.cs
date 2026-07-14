using CleanApi.Application.Common.Caching;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace CleanApi.Application.Common.Behaviors;

/// <summary>
/// Caches responses of queries that implement <see cref="ICacheableQuery"/> using HybridCache.
/// Because of the generic constraint, MediatR only applies this behavior to cacheable queries.
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse>(HybridCache cache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var options = request.Expiration is { } expiration
            ? new HybridCacheEntryOptions { Expiration = expiration }
            : null;

        return await cache.GetOrCreateAsync(
            request.CacheKey,
            async _ => await next(),
            options,
            request.Tags is { } tags ? [.. tags] : null,
            cancellationToken);
    }
}
