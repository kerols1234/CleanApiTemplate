using CleanApi.Application.Common.Interfaces;
using CleanApi.Domain.StoredProcs;
using Microsoft.Extensions.Logging;

namespace CleanApi.Infrastructure.Jobs;

/// <summary>
/// Example Hangfire job. Registered as a recurring job in Program.cs; can also be enqueued
/// fire-and-forget. Resolved from DI per run, so it can use scoped services.
/// </summary>
public sealed class SampleRecurringJob(IApplicationDbContext context, ILogger<SampleRecurringJob> logger)
{
    public async Task ReportLowStockAsync()
    {
        var lowStock = await context.QueryStoredProcAsync<LowStockProductResult>(
            "EXEC usp_GetLowStockProducts @Threshold = {0}",
            CancellationToken.None,
            10);

        logger.LogInformation("[Hangfire] Low-stock report: {Count} product(s) at or below threshold.", lowStock.Count);
    }
}
