using CleanApi.Domain.Common;

namespace CleanApi.Domain.StoredProcs;

/// <summary>
/// Row shape returned by the <c>usp_GetLowStockProducts</c> stored procedure.
/// Keyless (implements <see cref="IReadOnlyStoredProc"/>); called via
/// <c>IApplicationDbContext.QueryStoredProcAsync</c>.
/// </summary>
public class LowStockProductResult : IReadOnlyStoredProc
{
    public int ProductId { get; set; }

    public string Name { get; set; } = default!;

    public string Sku { get; set; } = default!;

    public int StockQuantity { get; set; }
}
