using Domain;

namespace Domain.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Constructor_Throws_WhenAmountIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Money(-1m, "USD"));
    }

    [Fact]
    public void Constructor_Throws_WhenCurrencyIsInvalid()
    {
        Assert.Throws<ArgumentException>(() => new Money(10m, "US"));
    }

    [Fact]
    public void Constructor_NormalizesCurrencyAndRoundsAmount()
    {
        Money money = new(10.125m, " usd ");

        Assert.Equal(10.12m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Equality_IsStructural()
    {
        Money left = new(42.10m, "eur");
        Money right = new(42.10m, "EUR");

        Assert.True(left == right);
    }

    [Fact]
    public void Properties_AreImmutable()
    {
        Assert.False(typeof(Money).GetProperty(nameof(Money.Amount))!.CanWrite);
        Assert.False(typeof(Money).GetProperty(nameof(Money.Currency))!.CanWrite);
    }
}
