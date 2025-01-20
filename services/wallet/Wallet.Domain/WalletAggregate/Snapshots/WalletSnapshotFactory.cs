using Wallet.BuildingBlocks.Domain;

namespace Wallet.Domain.WalletAggregate.Snapshots;

public class WalletSnapshotFactory:SnapshotFactory<Wallet,WalletSnapshot>
{
    public override int Interval => 10;
    protected override WalletSnapshot CreateSnapshot(Wallet aggregate) => new(aggregate.Balance, aggregate.OverUsedThreshold, aggregate.Owner, aggregate.CreatedAt);
}