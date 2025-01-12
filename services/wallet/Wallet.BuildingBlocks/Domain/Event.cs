namespace Wallet.BuildingBlocks.Domain;

public abstract record Event
{
    public long Index { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public Guid AggregateId { get; init; }
    public string AggregateType { get; init; }
    public string Type { get; init; }

    protected Event()
    {
        Type = GetType().Name;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

public record Event<TAggregate> : Event where TAggregate : Aggregate<TAggregate>, new()
{
    protected Event() => AggregateType = typeof(TAggregate).Name;

}

