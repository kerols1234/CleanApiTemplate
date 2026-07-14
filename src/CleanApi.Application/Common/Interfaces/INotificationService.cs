namespace CleanApi.Application.Common.Interfaces;

public sealed record PushNotification(string Title, string Body, IReadOnlyDictionary<string, string>? Data = null);

/// <summary>
/// Sends push notifications via Firebase Cloud Messaging (implemented in Infrastructure).
/// The implementation is a no-op unless a service-account file is configured, so the app runs
/// without Firebase credentials.
/// </summary>
public interface INotificationService
{
    /// <summary>Sends to specific device tokens. Returns the tokens FCM reported as invalid/stale.</summary>
    Task<IReadOnlyCollection<string>> SendToTokensAsync(
        IReadOnlyCollection<string> deviceTokens,
        PushNotification notification,
        CancellationToken cancellationToken = default);

    Task SendToTopicAsync(string topic, PushNotification notification, CancellationToken cancellationToken = default);
}
