// CS0618: FirebaseAdmin 3.x flags MulticastMessage.Tokens and GoogleCredential.FromFile as
// obsolete, but they remain the working path for token multicast + file-based service accounts
// (the suggested CredentialFactory replacement does not round-trip to GoogleCredential cleanly).
#pragma warning disable CS0618

using CleanApi.Application.Common.Interfaces;
using CleanApi.Infrastructure.Settings;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace CleanApi.Infrastructure.Services;

/// <summary>
/// Firebase Cloud Messaging implementation of <see cref="INotificationService"/>. Initialization is
/// LAZY and guarded: with no service-account file configured every method is a logged no-op, so the
/// application boots and runs without Firebase credentials.
/// </summary>
public sealed class FirebaseNotificationService(FirebaseSettings settings, ILogger<FirebaseNotificationService> logger)
    : INotificationService
{
    private static readonly Lock InitLock = new();
    private static FirebaseApp? _app;

    public async Task<IReadOnlyCollection<string>> SendToTokensAsync(
        IReadOnlyCollection<string> deviceTokens,
        PushNotification notification,
        CancellationToken cancellationToken = default)
    {
        var messaging = GetMessaging();
        if (messaging is null || deviceTokens.Count == 0)
        {
            logger.LogInformation("Firebase not configured or no tokens; skipping push to {Count} tokens.", deviceTokens.Count);
            return [];
        }

        var message = new MulticastMessage
        {
            Tokens = [.. deviceTokens],
            Notification = new Notification { Title = notification.Title, Body = notification.Body },
            Data = notification.Data is null ? null : new Dictionary<string, string>(notification.Data),
        };

        var response = await messaging.SendEachForMulticastAsync(message, cancellationToken);

        // Report tokens FCM rejected so the caller can prune them.
        var invalidTokens = new List<string>();
        for (var i = 0; i < response.Responses.Count; i++)
        {
            if (!response.Responses[i].IsSuccess)
            {
                invalidTokens.Add(message.Tokens[i]);
            }
        }

        return invalidTokens;
    }

    public async Task SendToTopicAsync(string topic, PushNotification notification, CancellationToken cancellationToken = default)
    {
        var messaging = GetMessaging();
        if (messaging is null)
        {
            logger.LogInformation("Firebase not configured; skipping push to topic {Topic}.", topic);
            return;
        }

        var message = new Message
        {
            Topic = topic,
            Notification = new Notification { Title = notification.Title, Body = notification.Body },
            Data = notification.Data is null ? null : new Dictionary<string, string>(notification.Data),
        };

        await messaging.SendAsync(message, cancellationToken);
    }

    private FirebaseMessaging? GetMessaging()
    {
        if (!settings.Enabled)
        {
            return null;
        }

        if (_app is null)
        {
            lock (InitLock)
            {
                _app ??= FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(settings.ServiceAccountPath),
                });
            }
        }

        return FirebaseMessaging.GetMessaging(_app);
    }
}
