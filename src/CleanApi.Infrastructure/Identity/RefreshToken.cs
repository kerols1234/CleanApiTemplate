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

    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;
}
