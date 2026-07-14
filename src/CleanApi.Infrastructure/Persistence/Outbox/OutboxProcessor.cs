using System.Text.Json;
using CleanApi.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanApi.Infrastructure.Persistence.Outbox;

/// <summary>
/// Polls the outbox and publishes unprocessed domain events through MediatR, marking each processed.
/// Provides at-least-once delivery; handlers should therefore be idempotent.
/// </summary>
public sealed class OutboxProcessor(IServiceProvider services, ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox processor started.");
        using var timer = new PeriodicTimer(PollInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Normal on shutdown.
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox processing batch failed.");
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var messages = await context.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var eventType = ResolveType(message.Type)
                    ?? throw new InvalidOperationException($"Cannot resolve event type '{message.Type}'.");

                var domainEvent = (INotification)JsonSerializer.Deserialize(message.Content, eventType)!;
                await publisher.Publish(domainEvent, cancellationToken);

                message.ProcessedOnUtc = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                message.Attempts++;
                message.Error = ex.Message;
                logger.LogError(ex, "Failed to publish outbox message {MessageId} ({Type}).", message.Id, message.Type);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static Type? ResolveType(string fullName) =>
        Type.GetType(fullName)
        ?? AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(fullName))
            .FirstOrDefault(type => type is not null);
}
