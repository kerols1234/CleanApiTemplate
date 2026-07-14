namespace CleanApi.Infrastructure.Settings;

/// <summary>
/// Firebase Cloud Messaging configuration. Bound from "Firebase". When
/// <see cref="ServiceAccountPath"/> is empty the notification service becomes a no-op, so the app
/// runs without Firebase credentials.
/// </summary>
public sealed class FirebaseSettings
{
    public const string SectionName = "Firebase";

    /// <summary>Path to the service-account JSON. Keep it out of source control (.gitignore'd).</summary>
    public string? ServiceAccountPath { get; init; }

    public bool Enabled => !string.IsNullOrWhiteSpace(ServiceAccountPath);
}
