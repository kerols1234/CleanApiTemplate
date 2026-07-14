using System.ComponentModel.DataAnnotations;

namespace CleanApi.Infrastructure.Settings;

/// <summary>JWT bearer configuration. Bound from the "Jwt" section; validated on startup.</summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = default!;

    [Required]
    public string Audience { get; init; } = default!;

    /// <summary>HMAC signing key. MUST come from user-secrets (dev) or a secret store (prod) — never appsettings.</summary>
    [Required]
    [MinLength(32)]
    public string SigningKey { get; init; } = default!;

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; init; } = 15;

    [Range(1, 365)]
    public int RefreshTokenDays { get; init; } = 7;
}
