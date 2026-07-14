using Microsoft.AspNetCore.Authorization;

namespace CleanApi.Api.Authorization;

/// <summary>
/// Requires the caller's token to carry a specific permission claim.
/// Usage: <c>[HasPermission(Permissions.Products.Create)]</c>.
/// </summary>
public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute(permission);
