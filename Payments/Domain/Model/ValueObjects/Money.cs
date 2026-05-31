namespace Frock_backend.Payments.Domain.Model.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public Money(decimal Amount, string Currency = "PEN")
    {
        if (Amount < 0)
            throw new ArgumentException("Amount must be greater than or equal to zero", nameof(Amount));
        if (string.IsNullOrWhiteSpace(Currency))
            throw new ArgumentException("Currency cannot be empty", nameof(Currency));

        this.Amount = Amount;
        this.Currency = Currency;
    }
}
