using PaymentService.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace PaymentService.Domain.Tests.ValueObjectTests;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldCreateMoney()
    {
        // Arrange
        var amount = 100.50m;
        var currency = "USD";

        // Act
        var money = new Money(amount, currency);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency);
    }

    [Fact]
    public void Constructor_WithDefaultCurrency_ShouldUseDKK()
    {
        // Arrange
        var amount = 100.00m;

        // Act
        var money = new Money(amount);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be("DKK");
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var negativeAmount = -50.00m;

        // Act
        var act = () => new Money(negativeAmount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Amount cannot be negative.*")
            .WithParameterName("amount");
    }

    [Fact]
    public void Constructor_WithZeroAmount_ShouldSucceed()
    {
        // Arrange
        var zeroAmount = 0.00m;

        // Act
        var money = new Money(zeroAmount);

        // Assert
        money.Amount.Should().Be(0.00m);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(99.99)]
    [InlineData(1000.50)]
    [InlineData(9999999.99)]
    public void Constructor_WithVariousValidAmounts_ShouldSucceed(decimal amount)
    {
        // Act
        var money = new Money(amount);

        // Assert
        money.Amount.Should().Be(amount);
    }

    [Fact]
    public void Money_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Money_WithDifferentAmounts_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(200.00m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Money_WithDifferentCurrencies_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "EUR");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_FromDecimal_ShouldCreateMoneyWithDKK()
    {
        // Arrange
        decimal amount = 150.75m;

        // Act
        Money money = amount;

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be("DKK");
    }

    [Fact]
    public void GetHashCode_ForEqualMoney_ShouldBeSame()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "USD");

        // Act & Assert
        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("DKK")]
    [InlineData("JPY")]
    public void Money_WithVariousCurrencies_ShouldStoreCorrectly(string currency)
    {
        // Arrange & Act
        var money = new Money(100.00m, currency);

        // Assert
        money.Currency.Should().Be(currency);
    }

    [Fact]
    public void Money_AsRecordType_ShouldSupportWith()
    {
        // Arrange
        var original = new Money(100.00m, "USD");

        // Act
        var modified = original with { Amount = 200.00m };

        // Assert
        modified.Amount.Should().Be(200.00m);
        modified.Currency.Should().Be("USD");
        original.Amount.Should().Be(100.00m); // Original should be unchanged
    }
}
