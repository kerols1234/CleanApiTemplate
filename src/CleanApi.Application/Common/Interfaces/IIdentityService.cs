using CleanApi.Application.Common.Models;

namespace CleanApi.Application.Common.Interfaces;

/// <summary>Tokens + basic profile returned after a successful login/refresh.</summary>
public sealed record AuthenticationResult(
    string UserId,
    string Email,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    IReadOnlyCollection<string> Roles);

/// <summary>
/// Wraps ASP.NET Core Identity + JWT issuance behind the Application layer (implemented in
/// Infrastructure). Returns <see cref="Result{T}"/> so handlers translate outcomes uniformly.
/// </summary>
public interface IIdentityService
{
    Task<Result<string>> RegisterAsync(string email, string password, CancellationToken cancellationToken);

    Task<Result<AuthenticationResult>> LoginAsync(string email, string password, CancellationToken cancellationToken);

    Task<Result<AuthenticationResult>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<Result> AssignRoleAsync(string userId, string role, CancellationToken cancellationToken);
}
