using CleanApi.Domain.Common;

namespace CleanApi.Domain.Entities;

/// <summary>A product category. Simple reference entity used by <see cref="Product"/>.</summary>
public class Category : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = [];

    // IAuditableEntity
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
