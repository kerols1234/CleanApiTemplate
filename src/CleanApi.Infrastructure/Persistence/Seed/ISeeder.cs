namespace CleanApi.Infrastructure.Persistence.Seed;

/// <summary>Runs idempotent data seeding at startup. Implementations are discovered and invoked by the host.</summary>
public interface ISeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}
