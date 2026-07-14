using CleanApi.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanApi.Application.Common.Behaviors;

/// <summary>Logs a structured entry for every request, tagged with the current user.</summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Handling {RequestName} for user {UserId}", requestName, currentUser.UserId ?? "anonymous");

        var response = await next();

        logger.LogInformation("Handled {RequestName}", requestName);

        return response;
    }
}
