using Wallet.BuildingBlocks.Domain;

namespace Wallet.Domain.WalletAggregate.Snapshots;

public record WalletSnapshot(Money Balance, Money OverUsedThreshold, Owner Owner, DateTimeOffset CreatedAt)
    : Snapshot<Wallet>;