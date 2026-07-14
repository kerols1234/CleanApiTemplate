using CleanApi.Domain.Common;
using CleanApi.Domain.Entities;

namespace CleanApi.Domain.Events;

/// <summary>Raised when a <see cref="Product"/> is created. Handled in the Application layer.</summary>
public sealed class ProductCreatedEvent(Product product) : IDomainEvent
{
    public Product Product { get; } = product;

    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
