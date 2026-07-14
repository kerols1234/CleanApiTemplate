namespace CleanApi.Infrastructure.Identity;

/// <summary>A persisted, rotatable refresh token bound to a user.</summary>
public class RefreshToken
{
    public int Id { get; set; }

    public string Token { get; set; } = default!;

    public string UserId { get; set; } = default!;

    public ApplicationUser User { get; set; } = default!;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>The token this one was rotated into. Presenting a token that already has a
    /// replacement is treated as reuse (token theft) and triggers revoking the whole session.</summary>
    public string? ReplacedByToken { get; set; }

    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;
}
