using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using CleanApi.Application.Modules.Products.Specifications;
using CleanApi.Domain.Entities;
using CleanApi.Domain.Repositories;
using CleanApi.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanApi.Application.Modules.Products.Commands;

// Convention: one file per feature holds the Command (record), its Validator, and its Handler.

public sealed record CreateProductCommand(
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    string Currency,
    int CategoryId,
    int StockQuantity) : IRequest<Result<ProductDto>>;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateProductCommandHandler(
    IApplicationDbContext context,
    IRepository<Product> repository,
    ProductMapper mapper)
    : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (!await context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken))
        {
            return Result.NotFound<ProductDto>($"Category {request.CategoryId} was not found.");
        }

        if (await context.Products.AnyAsync(p => p.Sku == request.Sku, cancellationToken))
        {
            return Result.Conflict<ProductDto>($"A product with SKU '{request.Sku}' already exists.");
        }

        var product = Product.Create(
            request.Name,
            request.Sku,
            new Money(request.Price, request.Currency),
            request.CategoryId,
            request.StockQuantity,
            request.Description);

        // Ardalis RepositoryBase.AddAsync persists and (via the SaveChanges interceptor) dispatches
        // the ProductCreatedEvent raised inside Product.Create.
        await repository.AddAsync(product, cancellationToken);

        var created = await repository.FirstOrDefaultAsync(new ProductByIdSpec(product.Id), cancellationToken) ?? product;

        return Result.Created(mapper.ToDto(created), "Product created successfully.");
    }
}
