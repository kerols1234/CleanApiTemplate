using CleanApi.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanApi.Application.Modules.Products.Events;

/// <summary>
/// Handles the <see cref="ProductCreatedEvent"/> domain event (dispatched through MediatR by the
/// DbContext after save). A real app might send a notification or enqueue a downstream job here.
/// </summary>
public sealed class ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product created: {Sku} ({Name})",
            notification.Sku,
            notification.Name);

        return Task.CompletedTask;
    }
}
