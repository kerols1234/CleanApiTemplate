using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using CleanApi.Domain.StoredProcs;
using FluentValidation;
using MediatR;

namespace CleanApi.Application.Modules.Products.Queries;

public sealed record LowStockProductDto(int ProductId, string Name, string Sku, int StockQuantity);

/// <summary>Returns low-stock products by calling the <c>usp_GetLowStockProducts</c> stored procedure.</summary>
public sealed record GetLowStockProductsQuery(int Threshold = 10) : IRequest<Result<IReadOnlyList<LowStockProductDto>>>;

public sealed class GetLowStockProductsQueryValidator : AbstractValidator<GetLowStockProductsQuery>
{
    public GetLowStockProductsQueryValidator() => RuleFor(x => x.Threshold).InclusiveBetween(0, 100_000);
}

public sealed class GetLowStockProductsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetLowStockProductsQuery, Result<IReadOnlyList<LowStockProductDto>>>
{
    public async Task<Result<IReadOnlyList<LowStockProductDto>>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        var rows = await context.QueryStoredProcAsync<LowStockProductResult>(
            "EXEC usp_GetLowStockProducts @Threshold = {0}",
            cancellationToken,
            request.Threshold);

        var dtos = rows
            .Select(r => new LowStockProductDto(r.ProductId, r.Name, r.Sku, r.StockQuantity))
            .ToList();

        return Result.Success<IReadOnlyList<LowStockProductDto>>(dtos);
    }
}
