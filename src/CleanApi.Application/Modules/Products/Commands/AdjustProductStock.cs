using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using CleanApi.Application.Modules.Products.Queries;
using CleanApi.Application.Modules.Products.Specifications;
using CleanApi.Domain.Entities;
using CleanApi.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace CleanApi.Application.Modules.Products.Commands;

public sealed record AdjustProductStockCommand(int Id, int Delta) : IRequest<Result<ProductDto>>;

public sealed class AdjustProductStockCommandValidator : AbstractValidator<AdjustProductStockCommand>
{
    public AdjustProductStockCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Delta).NotEqual(0).WithMessage("Delta must be non-zero.");
    }
}

public sealed class AdjustProductStockCommandHandler(
    IApplicationDbContext context,
    IRepository<Product> repository,
    HybridCache cache,
    ProductMapper mapper)
    : IRequestHandler<AdjustProductStockCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(AdjustProductStockCommand request, CancellationToken cancellationToken)
    {
        // Wrapped in an execution-strategy-safe transaction to demonstrate the pattern. A real
        // multi-write operation (e.g. transferring stock between products) would need this.
        return await context.ExecuteInTransactionAsync(async ct =>
        {
            var product = await repository.FirstOrDefaultAsync(new ProductByIdSpec(request.Id), ct);
            if (product is null)
            {
                return Result.NotFound<ProductDto>($"Product {request.Id} was not found.");
            }

            try
            {
                product.AdjustStock(request.Delta);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Conflict<ProductDto>(ex.Message);
            }

            await repository.SaveChangesAsync(ct);
            await cache.RemoveByTagAsync(ProductCacheTags.Products, ct);

            return Result.Success(mapper.ToDto(product), "Stock adjusted successfully.");
        }, cancellationToken);
    }
}
