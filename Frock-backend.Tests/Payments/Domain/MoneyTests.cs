using Frock_backend.Payments.Domain.Model.ValueObjects;

namespace Frock_backend.Tests.Payments.Domain;

public class MoneyTests
{
    [Fact]
    public void Money_Ctor_Throws_When_Amount_Negative()
    {
        // ARRANGE + ACT + ASSERT
        var ex = Assert.Throws<ArgumentException>(() => new Money(-1m, "PEN"));
        Assert.Equal("Amount", ex.ParamName);
    }

    [Fact]
    public void Money_Ctor_Defaults_Currency_To_PEN()
    {
        // ARRANGE + ACT
        var money = new Money(10m);

        // ASSERT
        Assert.Equal("PEN", money.Currency);
        Assert.Equal(10m, money.Amount);
    }

    [Fact]
    public void Money_Ctor_Accepts_Zero_Amount()
    {
        // ARRANGE + ACT
        var money = new Money(0m, "USD");

        // ASSERT
        Assert.Equal(0m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }
}
