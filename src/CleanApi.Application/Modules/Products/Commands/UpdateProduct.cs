using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using CleanApi.Application.Modules.Products.Queries;
using CleanApi.Application.Modules.Products.Specifications;
using CleanApi.Domain.Entities;
using CleanApi.Domain.Repositories;
using CleanApi.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace CleanApi.Application.Modules.Products.Commands;

public sealed record UpdateProductCommand(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int CategoryId) : IRequest<Result<ProductDto>>;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}

public sealed class UpdateProductCommandHandler(
    IApplicationDbContext context,
    IRepository<Product> repository,
    HybridCache cache,
    ProductMapper mapper)
    : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.FirstOrDefaultAsync(new ProductByIdSpec(request.Id), cancellationToken);
        if (product is null)
        {
            return Result.NotFound<ProductDto>($"Product {request.Id} was not found.");
        }

        if (!await context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken))
        {
            return Result.NotFound<ProductDto>($"Category {request.CategoryId} was not found.");
        }

        product.UpdateDetails(
            request.Name,
            request.Description,
            new Money(request.Price, request.Currency),
            request.CategoryId);

        await repository.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync(ProductCacheTags.Products, cancellationToken);

        return Result.Success(mapper.ToDto(product), "Product updated successfully.");
    }
}
