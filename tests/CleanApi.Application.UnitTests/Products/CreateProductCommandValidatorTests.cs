using CleanApi.Application.Modules.Products.Commands;
using FluentValidation.TestHelper;

namespace CleanApi.Application.UnitTests.Products;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    private static CreateProductCommand Valid() =>
        new("Widget", "W-1", "desc", 9.99m, "USD", CategoryId: 1, StockQuantity: 10);

    [Fact]
    public void Valid_Command_PassesValidation()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Name_FailsValidation()
    {
        var result = _validator.TestValidate(Valid() with { Name = "" });

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void NonPositive_Price_FailsValidation()
    {
        var result = _validator.TestValidate(Valid() with { Price = 0m });

        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Wrong_Currency_Length_FailsValidation()
    {
        var result = _validator.TestValidate(Valid() with { Currency = "US" });

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }
}
