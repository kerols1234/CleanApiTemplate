namespace CleanApi.Infrastructure.Settings;

/// <summary>Startup data-seeding configuration (admin bootstrap user). Bound from "Seed".</summary>
public sealed class SeedSettings
{
    public const string SectionName = "Seed";

    public bool SeedSampleData { get; init; } = true;

    public string AdminEmail { get; init; } = "admin@example.com";

    /// <summary>Dev-only default. Override via user-secrets/env in any real environment.</summary>
    public string AdminPassword { get; init; } = "Admin123!$";
}
