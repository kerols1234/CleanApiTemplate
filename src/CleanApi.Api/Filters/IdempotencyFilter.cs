using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace CleanApi.Api.Filters;

/// <summary>
/// Makes an endpoint idempotent per <c>Idempotency-Key</c> request header: the first request runs
/// normally and its response is cached; retries with the same key replay the stored response instead
/// of executing again. Requests without the header behave normally.
/// </summary>
/// <remarks>
/// Backed by <see cref="IMemoryCache"/>, so the guarantee is per-instance. For a multi-instance
/// deployment back it with a distributed cache (Redis) instead.
/// </remarks>
public sealed class IdempotencyFilter(IMemoryCache cache) : IAsyncActionFilter
{
    public const string HeaderName = "Idempotency-Key";
    private static readonly TimeSpan Retention = TimeSpan.FromHours(24);

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        if (!request.Headers.TryGetValue(HeaderName, out var headerValues) ||
            string.IsNullOrWhiteSpace(headerValues.ToString()))
        {
            await next();
            return;
        }

        var userScope = context.HttpContext.User.Identity?.Name ?? "anonymous";
        var cacheKey = $"idem:{userScope}:{request.Method}:{request.Path}:{headerValues}";

        if (cache.TryGetValue(cacheKey, out CachedResponse? cached) && cached is not null)
        {
            context.Result = new ContentResult
            {
                StatusCode = cached.StatusCode,
                ContentType = "application/json",
                Content = cached.Body,
            };
            return;
        }

        var executed = await next();

        // Only cache successful object responses (2xx) so retries after a failure can proceed.
        if (executed.Result is ObjectResult { Value: not null } objectResult)
        {
            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            if (statusCode is >= 200 and < 300)
            {
                var body = System.Text.Json.JsonSerializer.Serialize(objectResult.Value);
                cache.Set(cacheKey, new CachedResponse(statusCode, body), Retention);
            }
        }
    }

    private sealed record CachedResponse(int StatusCode, string Body);
}
