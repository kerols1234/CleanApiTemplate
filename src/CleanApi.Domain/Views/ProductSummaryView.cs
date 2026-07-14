using CleanApi.Domain.Common;

namespace CleanApi.Domain.Views;

/// <summary>
/// Keyless projection backed by the SQL view <c>Vw_ProductSummary</c> (per-category stock/value
/// rollup). The view DDL is created in a migration; the mapping is wired automatically because
/// this type implements <see cref="IReadOnlyView"/> and carries <see cref="DbViewAttribute"/>.
/// </summary>
[DbView("Vw_ProductSummary")]
public class ProductSummaryView : IReadOnlyView
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = default!;

    public int ProductCount { get; set; }

    public int TotalStock { get; set; }

    public decimal TotalStockValue { get; set; }
}
