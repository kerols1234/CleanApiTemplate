using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanApi.Infrastructure.Jobs;

/// <summary>Drains <see cref="IBackgroundTaskQueue"/>, running each work item in its own DI scope.</summary>
public sealed class QueuedHostedService(
    IBackgroundTaskQueue taskQueue,
    IServiceProvider services,
    ILogger<QueuedHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Background task queue processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await taskQueue.DequeueAsync(stoppingToken);
                await using var scope = services.CreateAsyncScope();
                await workItem(scope.ServiceProvider, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Normal on shutdown.
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while executing a background work item.");
            }
        }
    }
}
