using System.Security.Claims;

namespace CleanApi.Infrastructure.Authentication;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>Issues signed JWT access tokens. Internal to Infrastructure.</summary>
public interface IJwtTokenGenerator
{
    AccessToken GenerateAccessToken(string userId, string email, IEnumerable<Claim> additionalClaims);

    string GenerateRefreshToken();
}
