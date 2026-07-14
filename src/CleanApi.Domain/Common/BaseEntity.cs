using System.ComponentModel.DataAnnotations.Schema;

namespace CleanApi.Domain.Common;

/// <summary>
/// Base class for all persisted entities. Carries the primary key and a buffer of
/// domain events that the persistence layer dispatches after a successful save.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised by this entity and not yet dispatched.</summary>
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
