namespace CleanApi.Application.Modules.Products;

/// <summary>Read model for a product returned by the API.</summary>
public sealed record ProductDto(
    int Id,
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    int CategoryId,
    string? CategoryName);
