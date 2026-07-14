using CleanApi.Api;
using CleanApi.Api.Authorization;
using CleanApi.Api.Middleware;
#if (UseOpenTelemetry)
using CleanApi.Api.Observability;
#endif
using CleanApi.Application;
using CleanApi.Infrastructure;
using CleanApi.Infrastructure.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using QuestPDF.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

// Container HEALTHCHECK entrypoint: `dotnet CleanApi.Api.dll --healthcheck` probes /health and exits.
// Works on chiseled/distroless images (needs only the dotnet runtime — no curl/wget).
if (args is ["--healthcheck"])
{
    using var probe = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
    try
    {
        var response = await probe.GetAsync("http://localhost:8080/health");
        return response.IsSuccessStatusCode ? 0 : 1;
    }
    catch
    {
        return 1;
    }
}

// QuestPDF Community license — must be set before any document is generated.
QuestPDF.Settings.License = LicenseType.Community;

// Bootstrap logger for anything that happens before the host is built.
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

#if (UseSentry)
    // Sentry is a no-op while the DSN is empty (safe default).
    builder.WebHost.UseSentry();
#endif

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddApiServices(builder.Configuration);

#if (UseOpenTelemetry)
    builder.Services.AddObservability(builder.Configuration);
#endif

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseStatusCodePages();

    app.UseSecurityHeaders();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options => options
            .WithTitle("CleanApi")
            .WithTheme(ScalarTheme.Purple));
    }

    app.UseCors(CleanApi.Api.DependencyInjection.CorsPolicyName);
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireDashboardAuthorizationFilter()],
    });

    // Recurring job registration.
    RecurringJob.AddOrUpdate<SampleRecurringJob>("low-stock-report", job => job.ReportLowStockAsync(), Cron.Daily);
    RecurringJob.AddOrUpdate<RefreshTokenCleanupJob>("refresh-token-cleanup", job => job.CleanupAsync(), Cron.Daily);

    await app.InitializeDatabaseAsync();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

return 0;

/// <summary>Exposed so integration tests can use <c>WebApplicationFactory&lt;Program&gt;</c>.</summary>
public partial class Program;
