# Adding a feature

The `Products` module is the reference. This guide adds a hypothetical `Orders` feature the same way. The convention: **one file per feature** holding the request record, its validator, and its handler.

## 1. Model the domain (if needed)

Add entities/value objects to `CleanApi.Domain/Entities`. Keep behavior on the entity (rich domain model) — use factory methods and mutators rather than public setters. Raise domain events for things other parts of the system care about:

```csharp
public static Order Create(int customerId, Money total)
{
    var order = new Order { CustomerId = customerId, Total = total, Status = OrderStatus.Pending };
    order.RaiseDomainEvent(new OrderPlacedEvent(order.Reference));
    return order;
}
```

Expose the aggregate to the Application layer via `IApplicationDbContext` (add a `DbSet<Order>`), and add an entity configuration in `CleanApi.Infrastructure/Persistence/Configurations`.

## 2. Write a command

Create `CleanApi.Application/Modules/Orders/Commands/PlaceOrder.cs`:

```csharp
public sealed record PlaceOrderCommand(int CustomerId, decimal Total, string Currency)
    : IRequest<Result<OrderDto>>;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.Total).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public sealed class PlaceOrderCommandHandler(IRepository<Order> repository, OrderMapper mapper)
    : IRequestHandler<PlaceOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(PlaceOrderCommand request, CancellationToken ct)
    {
        var order = Order.Create(request.CustomerId, new Money(request.Total, request.Currency));
        await repository.AddAsync(order, ct);
        return Result.Created(mapper.ToDto(order), "Order placed.");
    }
}
```

The validator is discovered and run automatically by `ValidationBehavior` — handlers never validate input.

## 3. Write a query

Reads project straight to a DTO for EF translation. Implement `ICacheableQuery` to cache the result and (on writes) evict the tag:

```csharp
public sealed record GetOrdersQuery : PagedRequest, IRequest<Result<PagedResult<OrderDto>>>, ICacheableQuery
{
    public string CacheKey => $"orders:p={Page}:s={Size}:q={Search}";
    public IReadOnlyCollection<string>? Tags => ["orders"];
}
```

For a single aggregate, use a `Specification<T>` with `IReadRepository<T>.FirstOrDefaultAsync(spec, ct)`.

## 4. Map with Mapperly

```csharp
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class OrderMapper
{
    public partial OrderDto ToDto(Order order);
}
```

Register it in `CleanApi.Application/DependencyInjection.cs`: `services.AddSingleton<OrderMapper>();`.

## 5. Expose it via a controller

```csharp
[ApiVersion("1.0")]
[Authorize]
public sealed class OrdersController : ApiControllerBase
{
    [HttpPost]
    [HasPermission(Permissions.Orders.Create)]
    public async Task<IActionResult> Place([FromBody] PlaceOrderCommand command, CancellationToken ct)
        => (await Mediator.Send(command, ct)).ToActionResult();
}
```

Add the permission to `CleanApi.Domain/Authorization/Permissions.cs` and grant it to roles in `RolePermissions`.

## 6. Migrate

```bash
dotnet ef migrations add AddOrders -p src/CleanApi.Infrastructure -s src/CleanApi.Api -o Persistence/Migrations
```

## 7. Test

- **Unit**: validator tests (`TestValidate`) and handler tests with NSubstitute mocks.
- **Architecture**: no action needed — the rules already cover new handlers.
- **Integration**: add a `WebApplicationFactory` test if the flow is worth end-to-end coverage.

## Conventions checklist

- Commands/queries/DTOs are `record`s; handlers/validators are `sealed class`.
- Write conditional validation as separate `RuleFor` statements (never `.When()` mid-chain).
- Invalidate relevant cache tags in write handlers.
- Return `Result` — throw only for truly exceptional situations.
