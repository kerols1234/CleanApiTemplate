using System.Security.Claims;
using CleanApi.Application.Common.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using DomainPermissions = CleanApi.Domain.Authorization.Permissions;

namespace CleanApi.Api.Services;

/// <summary>Resolves the current user from the HTTP request's authenticated principal.</summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => Principal?.FindFirstValue(JwtRegisteredClaimNames.Email);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];

    public IReadOnlyCollection<string> Permissions =>
        Principal?.FindAll(DomainPermissions.ClaimType).Select(c => c.Value).ToArray() ?? [];

    public string GetRequiredUserId() =>
        UserId ?? throw new InvalidOperationException("No authenticated user on the current request.");
}
