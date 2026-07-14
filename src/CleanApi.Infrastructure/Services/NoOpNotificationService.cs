using CleanApi.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanApi.Infrastructure.Services;

/// <summary>
/// Notification service used when Firebase is not included in the project. Logs and does nothing,
/// so <see cref="INotificationService"/> consumers keep working without a push provider.
/// </summary>
public sealed class NoOpNotificationService(ILogger<NoOpNotificationService> logger) : INotificationService
{
    public Task<IReadOnlyCollection<string>> SendToTokensAsync(
        IReadOnlyCollection<string> deviceTokens,
        PushNotification notification,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Push notifications are disabled; dropping message to {Count} token(s).", deviceTokens.Count);
        return Task.FromResult<IReadOnlyCollection<string>>([]);
    }

    public Task SendToTopicAsync(string topic, PushNotification notification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Push notifications are disabled; dropping message to topic {Topic}.", topic);
        return Task.CompletedTask;
    }
}
