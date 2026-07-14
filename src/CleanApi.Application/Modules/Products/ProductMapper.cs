using CleanApi.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace CleanApi.Application.Modules.Products;

/// <summary>
/// Compile-time (source-generated) mapper — Riok.Mapperly, no runtime reflection and no AutoMapper
/// licensing concerns. Nested paths flatten the <see cref="Money"/> value object and the category name.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ProductMapper
{
    [MapProperty("Price.Amount", nameof(ProductDto.Price))]
    [MapProperty("Price.Currency", nameof(ProductDto.Currency))]
    [MapProperty("Category.Name", nameof(ProductDto.CategoryName))]
    public partial ProductDto ToDto(Product product);
}
