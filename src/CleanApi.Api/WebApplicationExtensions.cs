using CleanApi.Infrastructure.Persistence;
using CleanApi.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace CleanApi.Api;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Optionally applies migrations (when <c>Database:ApplyMigrationsOnStartup</c> is true) and runs
    /// all seeders. Failures are logged, not fatal, so the app still starts if the DB is unreachable.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
            {
                var context = services.GetRequiredService<AppDbContext>();
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied.");
            }

            // On by default; integration tests can disable seeding to boot without a database.
            if (app.Configuration.GetValue("Database:RunSeedersOnStartup", defaultValue: true))
            {
                foreach (var seeder in services.GetServices<ISeeder>())
                {
                    await seeder.SeedAsync(CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database initialization was skipped or failed (is the database reachable?).");
        }
    }
}
