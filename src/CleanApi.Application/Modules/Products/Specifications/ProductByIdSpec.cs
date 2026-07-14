using Ardalis.Specification;
using CleanApi.Domain.Entities;

namespace CleanApi.Application.Modules.Products.Specifications;

/// <summary>Loads a single product with its category. Example of the specification pattern.</summary>
public sealed class ProductByIdSpec : Specification<Product>
{
    public ProductByIdSpec(int id)
    {
        Query
            .Where(p => p.Id == id)
            .Include(p => p.Category);
    }
}

/// <summary>Products whose stock is at or below a threshold, cheapest first.</summary>
public sealed class LowStockProductsSpec : Specification<Product>
{
    public LowStockProductsSpec(int threshold)
    {
        Query
            .Where(p => p.IsActive && p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity);
    }
}
