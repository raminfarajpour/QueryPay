using Billing.Domain.SeedWorks;

namespace Billing.Domain.Billing;

public sealed class Money : ValueObject<Money>, IComparable<Money>
{
    public Money(decimal amount)
    {
        Amount = amount;
    }

    private Money()
    {
    }

    public decimal Amount { get; init; }
    
    public static bool operator <(Money a, Money b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        return a.Amount < b.Amount;
    }

    public static bool operator >(Money a, Money b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        return a.Amount > b.Amount;
    }

    public static Money operator +(Money a, Money b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        return new Money(a.Amount + b.Amount);
    }

    public static Money operator -(Money a, Money b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        return new Money(a.Amount - b.Amount);
    }

    protected override IEnumerable<object>? GetEqualityComponents()
    {
        yield return Amount;
    }

    public int CompareTo(Money? other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Amount.CompareTo(other.Amount);
    }

    public override string ToString() =>$"{Math.Floor(Amount):N0}";
}