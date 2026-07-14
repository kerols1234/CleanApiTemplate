using CleanApi.Application.Common.Models;

namespace CleanApi.Application.Common.Interfaces;

/// <summary>Data needed to render the sample product-catalog PDF.</summary>
public sealed record ProductCatalogPdfModel(string Title, IReadOnlyList<ProductCatalogPdfRow> Rows);

public sealed record ProductCatalogPdfRow(string Name, string Sku, decimal Price, string Currency, int Stock);

/// <summary>Generates PDF documents (implemented with QuestPDF in Infrastructure).</summary>
public interface IPdfGenerator
{
    FileDto GenerateProductCatalog(ProductCatalogPdfModel model);
}
