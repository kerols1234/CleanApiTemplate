using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CleanApi.Application.Common.Interfaces;
using CleanApi.Infrastructure.Settings;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CleanApi.Infrastructure.Authentication;

public sealed class JwtTokenGenerator(JwtSettings settings, IDateTimeProvider dateTime) : IJwtTokenGenerator
{
    public AccessToken GenerateAccessToken(string userId, string email, IEnumerable<Claim> additionalClaims)
    {
        var expiresAt = dateTime.UtcNow.AddMinutes(settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(additionalClaims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = credentials,
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(descriptor);

        return new AccessToken(token, expiresAt);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
