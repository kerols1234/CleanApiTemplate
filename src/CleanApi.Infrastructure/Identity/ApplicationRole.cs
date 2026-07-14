using Microsoft.AspNetCore.Identity;

namespace CleanApi.Infrastructure.Identity;

/// <summary>Application role. Permissions are stored as role claims (see the seeder).</summary>
public class ApplicationRole : IdentityRole
{
    public ApplicationRole() { }

    public ApplicationRole(string roleName) : base(roleName) { }
}
