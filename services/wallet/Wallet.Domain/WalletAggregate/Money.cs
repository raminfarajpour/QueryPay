using Wallet.BuildingBlocks.Domain;

namespace Wallet.Domain.WalletAggregate;

public record Money : ValueObject<Money>
{
    private Money()
    {
    }

    public Money(decimal amount)
    {
        Amount = amount;
    }

    public decimal Amount { get; init; }

    public static bool operator <(Money a, Money b)
    {
        var leftSideAmount = a?.Amount ?? 0L;
        var rightSideAmount = b?.Amount ?? 0L;

        return leftSideAmount < rightSideAmount;
    }

    public static bool operator >(Money a, Money b)
    {
        var leftSideAmount = a?.Amount ?? 0L;
        var rightSideAmount = b?.Amount ?? 0L;
        return leftSideAmount > rightSideAmount;
    }

    public static Money operator +(Money a, Money b)
    {
        var leftSideAmount = a?.Amount ?? 0L;
        var rightSideAmount = b?.Amount ?? 0L;
        return new Money(leftSideAmount + rightSideAmount);
    }

    public static Money operator -(Money a, Money b)
    {
        var leftSideAmount = a?.Amount ?? 0L;
        var rightSideAmount = b?.Amount ?? 0L;

        return new Money(leftSideAmount - rightSideAmount);
    }

    public static explicit operator decimal(Money value) => value.Amount;
    public static explicit operator Money(decimal value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
    }
}