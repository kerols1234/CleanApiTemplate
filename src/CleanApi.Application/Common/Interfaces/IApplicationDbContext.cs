using CleanApi.Domain.Common;
using CleanApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanApi.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext that the Application layer depends on (the concrete
/// <c>AppDbContext</c> lives in Infrastructure). Exposes aggregate sets, keyless view/proc access,
/// unit-of-work save, and execution-strategy-safe transactions.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }

    DbSet<Category> Categories { get; }

    /// <summary>Query a keyless type mapped to a database VIEW.</summary>
    IQueryable<TView> View<TView>()
        where TView : class, IReadOnlyView;

    /// <summary>Execute a stored procedure / raw SQL returning a keyless result set.</summary>
    Task<List<TResult>> QueryStoredProcAsync<TResult>(string sql, CancellationToken cancellationToken, params object[] parameters)
        where TResult : class, IReadOnlyStoredProc;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Runs <paramref name="operation"/> inside a database transaction, wrapped in the EF Core
    /// execution strategy so it composes with connection-resiliency (EnableRetryOnFailure).
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken);

    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
