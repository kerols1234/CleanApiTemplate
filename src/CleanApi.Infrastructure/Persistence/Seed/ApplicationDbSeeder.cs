using System.Security.Claims;
using CleanApi.Domain.Authorization;
using CleanApi.Domain.Entities;
using CleanApi.Domain.ValueObjects;
using CleanApi.Infrastructure.Identity;
using CleanApi.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanApi.Infrastructure.Persistence.Seed;

/// <summary>Seeds roles + permission claims, the bootstrap admin user, and (optionally) sample data.</summary>
public sealed class ApplicationDbSeeder(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    AppDbContext context,
    SeedSettings settings,
    ILogger<ApplicationDbSeeder> logger)
    : ISeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await SeedRolesAndPermissionsAsync();
        await SeedAdminUserAsync();

        if (settings.SeedSampleData)
        {
            await SeedSampleDataAsync(cancellationToken);
        }
    }

    private async Task SeedRolesAndPermissionsAsync()
    {
        foreach (var roleName in Roles.All)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new ApplicationRole(roleName);
                await roleManager.CreateAsync(role);
                logger.LogInformation("Seeded role {Role}", roleName);
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);
            var existingPermissions = existingClaims
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var permission in RolePermissions.ForRole(roleName))
            {
                if (existingPermissions.Add(permission))
                {
                    await roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
                }
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        if (await userManager.FindByEmailAsync(settings.AdminEmail) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = settings.AdminEmail,
            Email = settings.AdminEmail,
            EmailConfirmed = true,
            DisplayName = "Administrator",
        };

        var result = await userManager.CreateAsync(admin, settings.AdminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, Roles.Administrator);
            logger.LogInformation("Seeded admin user {Email}", settings.AdminEmail);
        }
        else
        {
            logger.LogWarning("Failed to seed admin user: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task SeedSampleDataAsync(CancellationToken cancellationToken)
    {
        if (await context.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        var beverages = new Category { Name = "Beverages", Description = "Drinks and refreshments" };
        var hardware = new Category { Name = "Hardware", Description = "Tools and equipment" };
        context.Categories.AddRange(beverages, hardware);
        await context.SaveChangesAsync(cancellationToken);

        context.Products.AddRange(
            Product.Create("Sparkling Water 500ml", "BEV-001", new Money(1.20m, "USD"), beverages.Id, 500, "Carbonated spring water"),
            Product.Create("Cold Brew Coffee 250ml", "BEV-002", new Money(3.50m, "USD"), beverages.Id, 120, "Ready-to-drink cold brew"),
            Product.Create("Cordless Drill", "HW-001", new Money(89.99m, "USD"), hardware.Id, 30, "18V cordless drill"),
            Product.Create("Screwdriver Set", "HW-002", new Money(24.99m, "USD"), hardware.Id, 8, "12-piece precision set"));

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded sample categories and products");
    }
}
