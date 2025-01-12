namespace Wallet.BuildingBlocks.Domain;

public abstract record Snapshot
{
    public long Index { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public Guid AggregateId { get; init; }

    public string AggregateType { get; init; }
    public string Type { get; init; }

    protected Snapshot()
    {
        Type = GetType().Name;
        Timestamp = DateTimeOffset.Now;
    }
}

public abstract record Snapshot<TAggregate> : Snapshot where TAggregate : Aggregate<TAggregate>, new()
{
    protected Snapshot() => AggregateType = typeof(TAggregate).Name;

}