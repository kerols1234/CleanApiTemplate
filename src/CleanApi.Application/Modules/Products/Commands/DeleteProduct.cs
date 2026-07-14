using CleanApi.Application.Common.Models;
using CleanApi.Application.Modules.Products.Queries;
using CleanApi.Domain.Entities;
using CleanApi.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace CleanApi.Application.Modules.Products.Commands;

public sealed record DeleteProductCommand(int Id) : IRequest<Result>;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class DeleteProductCommandHandler(IRepository<Product> repository, HybridCache cache)
    : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result.NotFound($"Product {request.Id} was not found.");
        }

        // Soft-delete: the DbContext's SaveChanges override converts this to IsDeleted = true.
        await repository.DeleteAsync(product, cancellationToken);
        await cache.RemoveByTagAsync(ProductCacheTags.Products, cancellationToken);

        return Result.Success("Product deleted successfully.");
    }
}
