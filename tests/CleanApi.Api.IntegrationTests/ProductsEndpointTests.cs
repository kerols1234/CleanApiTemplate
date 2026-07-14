using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using CleanApi.Infrastructure.Persistence;
using CleanApi.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;

namespace CleanApi.Api.IntegrationTests;

/// <summary>
/// Full DB-backed tests using a real SQL Server in a Testcontainers container. Automatically
/// SKIPPED when Docker is unavailable, so the suite stays green in environments without Docker.
/// </summary>
public sealed class ProductsEndpointTests : IAsyncLifetime
{
    private MsSqlContainer? _sqlContainer;
    private bool _dockerAvailable;

    public async Task InitializeAsync()
    {
        try
        {
            // NOTE: Build() itself probes Docker, so it must live inside the try.
            _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
            await _sqlContainer.StartAsync();
            _dockerAvailable = true;
        }
        catch
        {
            _dockerAvailable = false; // Docker not available — tests below no-op.
        }
    }

    public async Task DisposeAsync()
    {
        if (_sqlContainer is not null)
        {
            await _sqlContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task Login_ThenListProducts_ReturnsSeededData()
    {
        if (!_dockerAvailable)
        {
            return; // Skipped: Docker not available.
        }

        await using var factory = new DbBackedFactory(_sqlContainer!.GetConnectionString());

        // Migrate + seed explicitly (rather than relying on the app's startup path, which
        // WebApplicationFactory does not run deterministically).
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();

            foreach (var seeder in scope.ServiceProvider.GetServices<ISeeder>())
            {
                await seeder.SeedAsync(CancellationToken.None);
            }
        }

        var client = factory.CreateClient();

        // Seeded admin can log in.
        var login = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@example.com", password = "Admin123!$" });
        login.EnsureSuccessStatusCode();

        using var loginDoc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = loginDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        token.Should().NotBeNullOrEmpty();

        // Authenticated request returns the seeded sample products.
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var list = await client.GetAsync("/api/v1/products");
        list.StatusCode.Should().Be(HttpStatusCode.OK);

        using var listDoc = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        listDoc.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
    }

    private sealed class DbBackedFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Disable encryption for the throwaway test container. Microsoft.Data.SqlClient encrypts
            // by default, and the container's self-signed cert fails the TLS handshake under Ubuntu's
            // OpenSSL 3.x on CI (TCP provider error 35). Plaintext is fine for an ephemeral test DB.
            var testConnectionString = new SqlConnectionStringBuilder(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true,
            }.ConnectionString;

            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = testConnectionString,
                ["ConnectionStrings:Redis"] = "",
                // The test migrates + seeds explicitly, so leave startup init off.
                ["Database:ApplyMigrationsOnStartup"] = "false",
                ["Database:RunSeedersOnStartup"] = "false",
            }));
        }
    }
}
