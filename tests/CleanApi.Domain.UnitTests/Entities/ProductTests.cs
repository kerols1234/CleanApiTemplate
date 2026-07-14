using AwesomeAssertions;
using CleanApi.Domain.Entities;
using CleanApi.Domain.Events;
using CleanApi.Domain.ValueObjects;

namespace CleanApi.Domain.UnitTests.Entities;

public sealed class ProductTests
{
    private static Product NewProduct(int stock = 10) =>
        Product.Create("Widget", "W-1", new Money(9.99m, "USD"), categoryId: 1, stockQuantity: stock, description: null);

    [Fact]
    public void Create_RaisesProductCreatedEvent()
    {
        var product = NewProduct();

        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreatedEvent>();
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AdjustStock_WithinBounds_UpdatesQuantity()
    {
        var product = NewProduct(stock: 10);

        product.AdjustStock(-4);

        product.StockQuantity.Should().Be(6);
    }

    [Fact]
    public void AdjustStock_BelowZero_Throws()
    {
        var product = NewProduct(stock: 3);

        var act = () => product.AdjustStock(-5);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ClearDomainEvents_EmptiesTheBuffer()
    {
        var product = NewProduct();

        product.ClearDomainEvents();

        product.DomainEvents.Should().BeEmpty();
    }
}
