namespace CleanApi.Domain.Authorization;

/// <summary>
/// Fine-grained permissions in "Resource.Action" form. Endpoints require a permission via
/// <c>[HasPermission(Permissions.Products.Create)]</c>; permissions are carried as claims in the
/// JWT and enforced by the custom authorization policy provider.
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permission";

    public static class Products
    {
        public const string Read = "Products.Read";
        public const string Create = "Products.Create";
        public const string Update = "Products.Update";
        public const string Delete = "Products.Delete";
        public const string Export = "Products.Export";
    }

    public static class Categories
    {
        public const string Read = "Categories.Read";
        public const string Manage = "Categories.Manage";
    }

    public static class Admin
    {
        public const string ManageUsers = "Admin.ManageUsers";
        public const string SendNotifications = "Admin.SendNotifications";
    }

    /// <summary>Every declared permission — used to register one authorization policy per permission.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        Products.Read, Products.Create, Products.Update, Products.Delete, Products.Export,
        Categories.Read, Categories.Manage,
        Admin.ManageUsers, Admin.SendNotifications,
    ];
}

/// <summary>Which permissions each seeded role receives.</summary>
public static class RolePermissions
{
    public static IReadOnlyList<string> ForRole(string role) => role switch
    {
        Roles.Administrator => Permissions.All,
        Roles.Manager =>
        [
            Permissions.Products.Read, Permissions.Products.Create, Permissions.Products.Update,
            Permissions.Products.Export, Permissions.Categories.Read, Permissions.Categories.Manage,
        ],
        Roles.User => [Permissions.Products.Read, Permissions.Categories.Read],
        _ => [],
    };
}
