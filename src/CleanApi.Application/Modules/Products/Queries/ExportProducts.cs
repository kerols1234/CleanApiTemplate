using CleanApi.Application.Common.Extensions;
using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanApi.Application.Modules.Products.Queries;

/// <summary>Exports products to an .xlsx workbook (ClosedXML via <see cref="IExcelGenerator"/>).</summary>
public sealed record ExportProductsQuery(int? CategoryId = null) : IRequest<Result<FileDto>>;

public sealed class ExportProductsQueryHandler(IApplicationDbContext context, IExcelGenerator excel)
    : IRequestHandler<ExportProductsQuery, Result<FileDto>>
{
    // Column key -> display header, in output order.
    private static readonly IReadOnlyDictionary<string, string> Headers = new Dictionary<string, string>
    {
        ["Sku"] = "SKU",
        ["Name"] = "Name",
        ["Category"] = "Category",
        ["Price"] = "Price",
        ["Currency"] = "Currency",
        ["Stock"] = "Stock",
        ["IsActive"] = "Active",
    };

    public async Task<Result<FileDto>> Handle(ExportProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await context.Products
            .AsNoTracking()
            .WhereIf(request.CategoryId.HasValue, p => p.CategoryId == request.CategoryId!.Value)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Sku,
                p.Name,
                Category = p.Category!.Name,
                Price = p.Price.Amount,
                p.Price.Currency,
                Stock = p.StockQuantity,
                p.IsActive,
            })
            .ToListAsync(cancellationToken);

        var rows = products.Select(p => (IReadOnlyDictionary<string, object?>)new Dictionary<string, object?>
        {
            ["Sku"] = p.Sku,
            ["Name"] = p.Name,
            ["Category"] = p.Category,
            ["Price"] = p.Price,
            ["Currency"] = p.Currency,
            ["Stock"] = p.Stock,
            ["IsActive"] = p.IsActive ? "Yes" : "No",
        });

        var file = excel.Export("Products", "products.xlsx", Headers, rows);

        return Result.Success(file);
    }
}
