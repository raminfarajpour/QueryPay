using Wallet.BuildingBlocks.Domain;

namespace Wallet.Domain.WalletAggregate;

public record TransactionInfo(string TransactionId, string ReferenceId, string Description) : ValueObject<TransactionInfo>
{
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TransactionId;
        yield return ReferenceId;
        yield return Description;
    }
}