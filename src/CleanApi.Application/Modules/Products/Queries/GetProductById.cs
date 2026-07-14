using CleanApi.Application.Common.Models;
using CleanApi.Application.Modules.Products.Specifications;
using CleanApi.Domain.Entities;
using CleanApi.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace CleanApi.Application.Modules.Products.Queries;

public sealed record GetProductByIdQuery(int Id) : IRequest<Result<ProductDto>>;

public sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class GetProductByIdQueryHandler(IReadRepository<Product> repository, ProductMapper mapper)
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await repository.FirstOrDefaultAsync(new ProductByIdSpec(request.Id), cancellationToken);

        return product is null
            ? Result.NotFound<ProductDto>($"Product {request.Id} was not found.")
            : Result.Success(mapper.ToDto(product));
    }
}
