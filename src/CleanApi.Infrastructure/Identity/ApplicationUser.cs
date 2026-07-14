using Microsoft.AspNetCore.Identity;

namespace CleanApi.Infrastructure.Identity;

/// <summary>Application user. Extends IdentityUser with a display name and refresh tokens.</summary>
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
