namespace Wallet.BuildingBlocks.Domain;

public abstract record Event()
{
    public long Index { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public Guid AggregateId { get; init; }

    public string AggregateType { get; init; }
}

public record Event<TAggregate> : Event where TAggregate : Aggregate<TAggregate>, new()
{
    public Event() => AggregateType = typeof(TAggregate).Name;

    internal Event<TAggregate>? _previousEvent { get; set; }
}