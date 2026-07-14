using CleanApi.Application.Common.Interfaces;
using CleanApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanApi.Infrastructure.Jobs;

/// <summary>Deletes refresh tokens that are expired or were revoked more than a retention window ago.</summary>
public sealed class RefreshTokenCleanupJob(AppDbContext context, IDateTimeProvider dateTime, ILogger<RefreshTokenCleanupJob> logger)
{
    private static readonly TimeSpan RevokedRetention = TimeSpan.FromDays(7);

    public async Task CleanupAsync()
    {
        var now = dateTime.UtcNow;
        var revokedCutoff = now - RevokedRetention;

        var deleted = await context.RefreshTokens
            .Where(t => t.ExpiresAt < now || (t.RevokedAt != null && t.RevokedAt < revokedCutoff))
            .ExecuteDeleteAsync();

        if (deleted > 0)
        {
            logger.LogInformation("[Hangfire] Purged {Count} expired/revoked refresh token(s).", deleted);
        }
    }
}
