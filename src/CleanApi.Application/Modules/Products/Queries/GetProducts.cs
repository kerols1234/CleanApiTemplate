using CleanApi.Application.Common.Caching;
using CleanApi.Application.Common.Extensions;
using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanApi.Application.Modules.Products.Queries;

/// <summary>
/// Paged, filterable product list. Implements <see cref="ICacheableQuery"/> so the
/// <c>CachingBehavior</c> serves repeat calls from HybridCache for 60s (tagged "products").
/// </summary>
public sealed record GetProductsQuery : PagedRequest, IRequest<Result<PagedResult<ProductDto>>>, ICacheableQuery
{
    public int? CategoryId { get; init; }

    public bool? IsActive { get; init; }

    public string CacheKey =>
        $"products:p={Page}:s={Size}:sf={SortField}:so={SortOrder}:q={Search}:cat={CategoryId}:act={IsActive}";

    public TimeSpan? Expiration => TimeSpan.FromSeconds(60);

    public IReadOnlyCollection<string>? Tags => ProductCacheTags.All;
}

/// <summary>Cache tags for product queries, so writes can invalidate the whole group.</summary>
public static class ProductCacheTags
{
    public const string Products = "products";
    public static readonly string[] All = [Products];
}

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    private static readonly string[] AllowedSortFields = ["Name", "Sku", "StockQuantity", "CreatedAt"];

    public GetProductsQueryValidator()
    {
        RuleFor(x => x.SortField)
            .Must(f => string.IsNullOrWhiteSpace(f) || AllowedSortFields.Contains(f))
            .WithMessage($"SortField must be one of: {string.Join(", ", AllowedSortFields)}.");
    }
}

public sealed class GetProductsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductsQuery, Result<PagedResult<ProductDto>>>
{
    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Products
            .AsNoTracking()
            .WhereIf(request.CategoryId.HasValue, p => p.CategoryId == request.CategoryId!.Value)
            .WhereIf(request.IsActive.HasValue, p => p.IsActive == request.IsActive!.Value)
            .WhereIf(
                !string.IsNullOrWhiteSpace(request.Search),
                p => p.Name.Contains(request.Search!) || p.Sku.Contains(request.Search!));

        // A stable order is required before Skip/Take; default to Id when no sort is requested.
        query = string.IsNullOrWhiteSpace(request.SortField)
            ? query.OrderByDescending(p => p.Id)
            : query.ApplySort(request);

        var projected = query.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Sku,
            p.Price.Amount,
            p.Price.Currency,
            p.StockQuantity,
            p.IsActive,
            p.CategoryId,
            p.Category!.Name));

        var page = await projected.ToPagedResultAsync(request.Page, request.Size, cancellationToken);

        return Result.Success(page);
    }
}
