using MediatR;

namespace CleanApi.Domain.Common;

/// <summary>
/// Marker for a domain event. Extends <see cref="INotification"/> so events can be
/// published through MediatR by the <c>DispatchDomainEventsInterceptor</c> after
/// changes are saved. Raise events from inside aggregates via <see cref="BaseEntity.RaiseDomainEvent"/>.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredOn { get; }
}
