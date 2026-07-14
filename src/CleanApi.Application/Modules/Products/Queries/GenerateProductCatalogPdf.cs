using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanApi.Application.Modules.Products.Queries;

/// <summary>Renders the product catalog to a PDF (QuestPDF via <see cref="IPdfGenerator"/>).</summary>
public sealed record GenerateProductCatalogPdfQuery : IRequest<Result<FileDto>>;

public sealed class GenerateProductCatalogPdfQueryHandler(IApplicationDbContext context, IPdfGenerator pdf)
    : IRequestHandler<GenerateProductCatalogPdfQuery, Result<FileDto>>
{
    public async Task<Result<FileDto>> Handle(GenerateProductCatalogPdfQuery request, CancellationToken cancellationToken)
    {
        var rows = await context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new ProductCatalogPdfRow(p.Name, p.Sku, p.Price.Amount, p.Price.Currency, p.StockQuantity))
            .ToListAsync(cancellationToken);

        var file = pdf.GenerateProductCatalog(new ProductCatalogPdfModel("Product Catalog", rows));

        return Result.Success(file);
    }
}
