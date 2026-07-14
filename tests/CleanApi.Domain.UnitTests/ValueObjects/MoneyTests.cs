using AwesomeAssertions;
using CleanApi.Domain.ValueObjects;

namespace CleanApi.Domain.UnitTests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_WithValidValues_SetsProperties()
    {
        var money = new Money(12.50m, "usd");

        money.Amount.Should().Be(12.50m);
        money.Currency.Should().Be("USD"); // normalized to upper-case
    }

    [Fact]
    public void Equality_IsByValue()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "USD");
        var c = new Money(10m, "EUR");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.Should().NotBe(c);
    }

    [Fact]
    public void Create_WithNegativeAmount_Throws()
    {
        var act = () => new Money(-1m, "USD");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("")]
    public void Create_WithInvalidCurrency_Throws(string currency)
    {
        var act = () => new Money(1m, currency);

        act.Should().Throw<ArgumentException>();
    }
}
