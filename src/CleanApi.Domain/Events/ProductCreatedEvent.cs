using CleanApi.Domain.Common;

namespace CleanApi.Domain.Events;

/// <summary>
/// Raised when a product is created. Carries only serializable primitive data (not the entity) so
/// it can be persisted to the outbox and published reliably; consumers resolve the product by SKU.
/// </summary>
public sealed class ProductCreatedEvent(string sku, string name) : IDomainEvent
{
    public string Sku { get; } = sku;

    public string Name { get; } = name;

    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
