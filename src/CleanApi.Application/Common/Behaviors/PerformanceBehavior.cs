using System.Diagnostics;
using CleanApi.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanApi.Application.Common.Behaviors;

/// <summary>Times each request and logs a warning when it exceeds the slow-request threshold.</summary>
public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long SlowRequestThresholdMs = 500;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();

        var response = await next();

        timer.Stop();

        if (timer.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            logger.LogWarning(
                "Long-running request: {RequestName} took {ElapsedMilliseconds} ms (user {UserId})",
                typeof(TRequest).Name,
                timer.ElapsedMilliseconds,
                currentUser.UserId ?? "anonymous");
        }

        return response;
    }
}
