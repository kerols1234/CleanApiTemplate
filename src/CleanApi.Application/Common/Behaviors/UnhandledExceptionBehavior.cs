using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanApi.Application.Common.Behaviors;

/// <summary>
/// Last-resort logging for exceptions that escape a handler. Re-throws so the API's global
/// exception handler still produces the HTTP response — this only guarantees a log entry.
/// </summary>
public sealed class UnhandledExceptionBehavior<TRequest, TResponse>(
    ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for request {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
