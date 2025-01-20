namespace Wallet.BuildingBlocks.Domain;

public abstract class SnapshotFactory<TAggregate, TSnapshot> : ISnapshotFactory<TAggregate>
    where TAggregate : Aggregate<TAggregate>, new()
    where TSnapshot : Snapshot<TAggregate>
{
    public abstract int Interval { get; }

    public bool ShouldCaptureSnapshot(TAggregate aggregate)
    {
        if (Interval == 0) return false;
        if (aggregate.Events.Count == 0) return false;

        var firstEventSegment = aggregate.Events.First().Index / Interval;
        var nextEventSegment = (aggregate.Events.Last().Index + 1) / Interval;
        return firstEventSegment != nextEventSegment;
    }

    protected abstract TSnapshot CreateSnapshot(TAggregate aggregate);

    public Snapshot<TAggregate> Create(TAggregate aggregate)
    {
        return CreateSnapshot(aggregate) with
        {
            AggregateId = aggregate.Id,
            AggregateType = aggregate.Type,
            Index = aggregate.Version - 1
        };
    }
}