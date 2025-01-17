using Wallet.BuildingBlocks.Domain;

namespace Wallet.Domain.WalletAggregate;

public record TransactionInfo(string ReferenceId, string Description) : ValueObject<TransactionInfo>
{
    public readonly string TransactionId= Guid.NewGuid().ToString("N");
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TransactionId;
        yield return ReferenceId;
        yield return Description;
    }
}