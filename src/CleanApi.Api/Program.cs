using CleanApi.Api;
using CleanApi.Api.Authorization;
using CleanApi.Application;
using CleanApi.Infrastructure;
using CleanApi.Infrastructure.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using QuestPDF.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

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

    // Sentry is a no-op while the DSN is empty (safe default).
    builder.WebHost.UseSentry();

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddApiServices(builder.Configuration);

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseStatusCodePages();

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

/// <summary>Exposed so integration tests can use <c>WebApplicationFactory&lt;Program&gt;</c>.</summary>
public partial class Program;
