using Wallet.BuildingBlocks.Domain;

namespace Wallet.Domain.WalletAggregate;

public record Owner: ValueObject<Owner>
{
    private Owner()
    {
    }

    public Owner(long userId, string mobile)
    {
        UserId = userId;
        Mobile = mobile;
    }
    public long UserId { get; init; }
    public string Mobile { get; init; }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return UserId;
        yield return Mobile;
    }
}