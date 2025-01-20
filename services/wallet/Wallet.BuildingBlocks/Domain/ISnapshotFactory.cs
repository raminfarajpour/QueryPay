namespace Wallet.BuildingBlocks.Domain;

public interface ISnapshotFactory<TAggregate> where TAggregate : Aggregate<TAggregate>, new()
{
    int Interval { get; }
    bool ShouldCaptureSnapshot(TAggregate aggregate);
    Snapshot<TAggregate> Create(TAggregate aggregate);
}