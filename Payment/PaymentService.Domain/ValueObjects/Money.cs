namespace PaymentService.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public Money(decimal amount, string currency = "DKK")
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

        Amount = amount;
        Currency = currency;
    }

    // Allow implicit conversion from decimal to Money
    public static implicit operator Money(decimal value) => new(value);
}
