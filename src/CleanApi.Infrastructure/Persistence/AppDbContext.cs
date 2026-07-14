using System.Text.Json;
using CleanApi.Application.Common.Interfaces;
using CleanApi.Domain.Common;
using CleanApi.Domain.Entities;
using CleanApi.Infrastructure.Identity;
using CleanApi.Infrastructure.Persistence.Extensions;
using CleanApi.Infrastructure.Persistence.Outbox;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanApi.Infrastructure.Persistence;

/// <summary>
/// The application's EF Core context. Also serves as the unit of work behind
/// <see cref="IApplicationDbContext"/>. Cross-cutting persistence concerns (auditing, soft-delete,
/// domain-event dispatch) are applied in <see cref="SaveChangesAsync"/>.
/// </summary>
public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTime)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options), IApplicationDbContext
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var infrastructureAssembly = typeof(AppDbContext).Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(infrastructureAssembly);

        // Keyless views + stored-proc result types live in the Domain assembly.
        var domainAssembly = typeof(Product).Assembly;
        modelBuilder.ApplyDatabaseViews(domainAssembly);
        modelBuilder.ApplyStoredProcedureResults(domainAssembly);
    }

    public IQueryable<TView> View<TView>()
        where TView : class, IReadOnlyView
        => Set<TView>().AsNoTracking();

    public async Task<List<TResult>> QueryStoredProcAsync<TResult>(string sql, CancellationToken cancellationToken, params object[] parameters)
        where TResult : class, IReadOnlyStoredProc
        => await Set<TResult>().FromSqlRaw(sql, parameters).ToListAsync(cancellationToken);

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken)
    {
        var strategy = Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await Database.BeginTransactionAsync(ct);
            var result = await operation(ct);
            await transaction.CommitAsync(ct);
            return result;
        }, cancellationToken);
    }

    public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken)
        => ExecuteInTransactionAsync<object?>(async ct =>
        {
            await operation(ct);
            return null;
        }, cancellationToken);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        ConvertDeletesToSoftDeletes();

        // Persist domain events to the outbox in the SAME transaction as the state change.
        // They are published asynchronously by the OutboxProcessor (at-least-once delivery).
        WriteDomainEventsToOutbox();

        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var now = dateTime.UtcNow;
        var userId = currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = now;
                    entry.Entity.LastModifiedBy = userId;
                    break;
            }
        }
    }

    private void ConvertDeletesToSoftDeletes()
    {
        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = dateTime.UtcNow;
            }
        }
    }

    private void WriteDomainEventsToOutbox()
    {
        var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                OutboxMessages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().FullName!,
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredOnUtc = domainEvent.OccurredOn,
                });
            }

            entity.ClearDomainEvents();
        }
    }
}
