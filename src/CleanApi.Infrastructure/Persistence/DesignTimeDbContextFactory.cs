using CleanApi.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CleanApi.Infrastructure.Persistence;

/// <summary>
/// Lets <c>dotnet ef</c> create the context WITHOUT booting the full web host (which would require
/// JWT secrets, Redis, etc.). Used by <c>migrations add</c> / <c>database update</c>. Reads the
/// connection string from the <c>ConnectionStrings__Default</c> environment variable, falling back
/// to the local docker-compose SQL Server.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Server=localhost,1433;Database=CleanApi;User Id=sa;Password=Your_password123;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options, new DesignTimeCurrentUser(), new DesignTimeClock(), new DesignTimeNoOpPublisher());
    }

    private sealed class DesignTimeCurrentUser : ICurrentUserService
    {
        public string? UserId => null;
        public string? UserName => null;
        public bool IsAuthenticated => false;
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Permissions => [];
        public string GetRequiredUserId() => throw new InvalidOperationException("No user at design time.");
    }

    private sealed class DesignTimeClock : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }

    private sealed class DesignTimeNoOpPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;
    }
}
