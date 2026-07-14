using System.Security.Claims;
using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using CleanApi.Domain.Authorization;
using CleanApi.Infrastructure.Authentication;
using CleanApi.Infrastructure.Persistence;
using CleanApi.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanApi.Infrastructure.Identity;

/// <summary>ASP.NET Core Identity + JWT implementation of <see cref="IIdentityService"/>.</summary>
public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IJwtTokenGenerator tokenGenerator,
    IDateTimeProvider dateTime,
    JwtSettings jwtSettings,
    AppDbContext dbContext)
    : IIdentityService
{
    public async Task<Result<string>> RegisterAsync(string email, string password, CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return Result.Conflict<string>($"A user with email '{email}' already exists.");
        }

        var user = new ApplicationUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Result.Error<string>(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, Roles.User);

        return Result.Success(user.Id, "Registration successful.");
    }

    public async Task<Result<AuthenticationResult>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            return Result.Error<AuthenticationResult>("Invalid credentials.");
        }

        return Result.Success(await IssueTokensAsync(user, cancellationToken));
    }

    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var stored = await dbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

        if (stored is null || !stored.IsActive)
        {
            return Result.Error<AuthenticationResult>("Invalid or expired refresh token.");
        }

        // Rotate: revoke the presented token and issue a fresh pair.
        stored.RevokedAt = dateTime.UtcNow;

        var result = await IssueTokensAsync(stored.User, cancellationToken);
        return Result.Success(result);
    }

    public async Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var stored = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);
        if (stored is null)
        {
            return Result.NotFound("Refresh token not found.");
        }

        stored.RevokedAt = dateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success("Token revoked.");
    }

    public async Task<Result> AssignRoleAsync(string userId, string role, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result.NotFound($"User {userId} was not found.");
        }

        if (!await roleManager.RoleExistsAsync(role))
        {
            return Result.NotFound($"Role '{role}' does not exist.");
        }

        var result = await userManager.AddToRoleAsync(user, role);
        return result.Succeeded
            ? Result.Success($"Role '{role}' assigned.")
            : Result.Error(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    private async Task<AuthenticationResult> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var (roles, claims) = await BuildClaimsAsync(user);

        var accessToken = tokenGenerator.GenerateAccessToken(user.Id, user.Email!, claims);
        var refreshToken = tokenGenerator.GenerateRefreshToken();

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            CreatedAt = dateTime.UtcNow,
            ExpiresAt = dateTime.UtcNow.AddDays(jwtSettings.RefreshTokenDays),
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthenticationResult(
            user.Id,
            user.Email!,
            accessToken.Value,
            accessToken.ExpiresAt,
            refreshToken,
            roles);
    }

    private async Task<(IReadOnlyCollection<string> Roles, IReadOnlyCollection<Claim> Claims)> BuildClaimsAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>();
        var permissions = new HashSet<string>(StringComparer.Ordinal);

        foreach (var roleName in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, roleName));

            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            foreach (var claim in await roleManager.GetClaimsAsync(role))
            {
                if (claim.Type == Permissions.ClaimType)
                {
                    permissions.Add(claim.Value);
                }
            }
        }

        claims.AddRange(permissions.Select(p => new Claim(Permissions.ClaimType, p)));

        return (roles.ToList(), claims);
    }
}
