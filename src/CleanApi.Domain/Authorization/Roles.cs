namespace CleanApi.Domain.Authorization;

/// <summary>Built-in role names. Seeded at startup and granted the permissions in <see cref="RolePermissions"/>.</summary>
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Manager = "Manager";
    public const string User = "User";

    public static readonly IReadOnlyList<string> All = [Administrator, Manager, User];
}
