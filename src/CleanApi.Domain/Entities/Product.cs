using CleanApi.Domain.Common;
using CleanApi.Domain.Events;
using CleanApi.Domain.ValueObjects;

namespace CleanApi.Domain.Entities;

/// <summary>
/// Product aggregate root. Behaviour lives on the entity (rich domain model) rather than in
/// handlers: use the factory <see cref="Create"/> and the mutators below instead of setting
/// properties directly from the outside.
/// </summary>
public class Product : BaseEntity, IAuditableEntity, ISoftDeletable
{
    // Private setters — state changes go through methods so invariants hold.
    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    public string Sku { get; private set; } = default!;

    public Money Price { get; private set; } = default!;

    public int StockQuantity { get; private set; }

    public bool IsActive { get; private set; }

    public int CategoryId { get; private set; }

    public Category? Category { get; private set; }

    // IAuditableEntity
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    private Product() { } // EF

    public static Product Create(string name, string sku, Money price, int categoryId, int stockQuantity, string? description)
    {
        var product = new Product
        {
            Name = name,
            Sku = sku,
            Price = price,
            CategoryId = categoryId,
            StockQuantity = stockQuantity,
            Description = description,
            IsActive = true,
        };

        product.RaiseDomainEvent(new ProductCreatedEvent(product));
        return product;
    }

    public void UpdateDetails(string name, string? description, Money price, int categoryId)
    {
        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
    }

    /// <summary>Adjusts stock by a delta. Throws if it would drive stock negative.</summary>
    public void AdjustStock(int delta)
    {
        var newQuantity = StockQuantity + delta;
        if (newQuantity < 0)
        {
            throw new InvalidOperationException(
                $"Insufficient stock for '{Sku}': have {StockQuantity}, tried to change by {delta}.");
        }

        StockQuantity = newQuantity;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
