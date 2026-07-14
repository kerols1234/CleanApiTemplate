using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using CleanApi.Domain.Views;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanApi.Application.Modules.Products.Queries;

/// <summary>Per-category rollup, read from the SQL view <c>Vw_ProductSummary</c>.</summary>
public sealed record ProductCategorySummaryDto(
    int CategoryId,
    string CategoryName,
    int ProductCount,
    int TotalStock,
    decimal TotalStockValue);

public sealed record GetProductSummaryQuery : IRequest<Result<IReadOnlyList<ProductCategorySummaryDto>>>;

public sealed class GetProductSummaryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductSummaryQuery, Result<IReadOnlyList<ProductCategorySummaryDto>>>
{
    public async Task<Result<IReadOnlyList<ProductCategorySummaryDto>>> Handle(GetProductSummaryQuery request, CancellationToken cancellationToken)
    {
        var rows = await context.View<ProductSummaryView>()
            .OrderByDescending(v => v.TotalStockValue)
            .Select(v => new ProductCategorySummaryDto(
                v.CategoryId,
                v.CategoryName,
                v.ProductCount,
                v.TotalStock,
                v.TotalStockValue))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ProductCategorySummaryDto>>(rows);
    }
}
