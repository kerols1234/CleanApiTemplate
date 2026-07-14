using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CleanApi.Api.IntegrationTests;

/// <summary>
/// Boots the real application pipeline for pipeline-level tests (routing, auth, OpenAPI) WITHOUT a
/// database: migrations and seeders are disabled, so no external infrastructure is required.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development so the dev appsettings (incl. Jwt:SigningKey) loads and options validation passes.
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ApplyMigrationsOnStartup"] = "false",
                ["Database:RunSeedersOnStartup"] = "false",
                ["ConnectionStrings:Redis"] = "",
            });
        });
    }
}
