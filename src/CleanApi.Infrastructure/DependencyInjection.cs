using CleanApi.Application.Common.Interfaces;
using CleanApi.Domain.Repositories;
using CleanApi.Infrastructure.Authentication;
using CleanApi.Infrastructure.Identity;
using CleanApi.Infrastructure.Jobs;
using CleanApi.Infrastructure.Persistence;
using CleanApi.Infrastructure.Persistence.Outbox;
using CleanApi.Infrastructure.Persistence.Repositories;
using CleanApi.Infrastructure.Persistence.Seed;
using CleanApi.Infrastructure.Pdf;
using CleanApi.Infrastructure.Services;
using CleanApi.Infrastructure.Settings;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CleanApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddIdentityAndAuth(services, configuration);
        AddCaching(services, configuration);
        AddJobs(services, configuration);
        AddSettingsBoundServices(services, configuration);

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        // Scoped context (not pooled): it injects ICurrentUserService/IPublisher.
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure();
            }));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // Specification-based repositories (Ardalis).
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

        services.AddScoped<ISeeder, ApplicationDbSeeder>();
    }

    private static void AddIdentityAndAuth(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;

                // Account lockout after repeated failed logins.
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager();

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");

        // Register Redis as the L2 store ONLY when configured; otherwise HybridCache is L1-only.
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
        }

#pragma warning disable EXTEXP0018 // HybridCache is stable in .NET 9+; suppress the experimental gate.
        services.AddHybridCache();
#pragma warning restore EXTEXP0018
    }

    private static void AddJobs(IServiceCollection services, IConfiguration configuration)
    {
        var hangfireConnectionString = configuration.GetConnectionString("Hangfire")
            ?? configuration.GetConnectionString("Default");

        var useSqlServerStorage = !string.IsNullOrWhiteSpace(hangfireConnectionString)
            && configuration.GetValue("Hangfire:UseSqlServerStorage", defaultValue: false);

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings();

            if (useSqlServerStorage)
            {
                config.UseSqlServerStorage(hangfireConnectionString);
            }
            else
            {
                // Safe default: no external dependency, so the app always boots.
                config.UseInMemoryStorage();
            }
        });

        services.AddHangfireServer();
        services.AddScoped<SampleRecurringJob>();
        services.AddScoped<RefreshTokenCleanupJob>();

        // Channel-based in-process queue + its processor (the built-in alternative to Hangfire).
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedHostedService>();

        // Transactional outbox: publishes persisted domain events asynchronously.
        services.AddHostedService<OutboxProcessor>();
    }

    private static void AddSettingsBoundServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EmailSettings>().Bind(configuration.GetSection(EmailSettings.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<EmailSettings>>().Value);

#if (UseFirebase)
        services.AddOptions<FirebaseSettings>().Bind(configuration.GetSection(FirebaseSettings.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<FirebaseSettings>>().Value);
#endif

        services.AddOptions<SeedSettings>().Bind(configuration.GetSection(SeedSettings.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<SeedSettings>>().Value);

        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IExcelGenerator, ExcelGenerator>();
        services.AddSingleton<IPdfGenerator, QuestPdfGenerator>();
#if (UseFirebase)
        services.AddSingleton<INotificationService, FirebaseNotificationService>();
#else
        services.AddSingleton<INotificationService, NoOpNotificationService>();
#endif
    }
}
