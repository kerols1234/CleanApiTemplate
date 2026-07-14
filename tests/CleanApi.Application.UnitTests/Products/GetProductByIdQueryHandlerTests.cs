using Ardalis.Specification;
using AwesomeAssertions;
using CleanApi.Application.Common.Models;
using CleanApi.Application.Modules.Products;
using CleanApi.Application.Modules.Products.Queries;
using CleanApi.Domain.Entities;
using CleanApi.Domain.Repositories;
using CleanApi.Domain.ValueObjects;
using NSubstitute;

namespace CleanApi.Application.UnitTests.Products;

public sealed class GetProductByIdQueryHandlerTests
{
    private readonly IReadRepository<Product> _repository = Substitute.For<IReadRepository<Product>>();
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _handler = new GetProductByIdQueryHandler(_repository, new ProductMapper());
    }

    [Fact]
    public async Task Handle_WhenProductMissing_ReturnsNotFound()
    {
        _repository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var result = await _handler.Handle(new GetProductByIdQuery(99), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenProductExists_ReturnsMappedDto()
    {
        var product = Product.Create("Widget", "W-1", new Money(9.99m, "USD"), 1, 10, "desc");
        _repository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(product);

        var result = await _handler.Handle(new GetProductByIdQuery(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Sku.Should().Be("W-1");
        result.Value.Price.Should().Be(9.99m);
        result.Value.Currency.Should().Be("USD");
    }
}
