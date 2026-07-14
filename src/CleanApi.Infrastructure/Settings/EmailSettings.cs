namespace CleanApi.Infrastructure.Settings;

/// <summary>SMTP configuration for the MailKit-based email service. Bound from "Email".</summary>
public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 1025;

    public bool UseSsl { get; init; }

    public string? Username { get; init; }

    public string? Password { get; init; }

    public string FromAddress { get; init; } = "no-reply@example.com";

    public string FromName { get; init; } = "CleanApi";
}
