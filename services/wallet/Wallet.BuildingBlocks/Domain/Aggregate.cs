namespace Wallet.BuildingBlocks.Domain;

public abstract class Aggregate
{
    public Guid Id { get; init; }
    public string Type { get; init; }
    public long Version { get; set; }
    public abstract void ClearEvents();

    internal Aggregate()
    {
        Id = Guid.NewGuid();
        Type = GetType().Name;
    }
}

public abstract class Aggregate<TAggregate> : Aggregate where TAggregate : Aggregate<TAggregate>, new()
{
    private readonly List<Event<TAggregate>> _events = [];
    public IReadOnlyCollection<Event<TAggregate>> Events => _events;

    public TEvent Apply<TEvent>(TEvent e) where TEvent : Event<TAggregate>
    {
        e = e with
        {
            AggregateId = Id,
            AggregateType = Type,
            Index = Version,
        };

        ValidateAndApply(e);
        _events.Add(e);
        return e;
    }

    protected abstract void Apply(Event<TAggregate> @event);

    protected virtual void Apply(Snapshot<TAggregate> snapshot)
    {
    }

    private void ValidateAndApply(Event<TAggregate>? @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        Validate(@event);
        Apply(@event);
        Version++;
    }

    private void ValidateAndApply(Snapshot<TAggregate> snapshot)
    {
        Validate(snapshot);
        Apply(snapshot);
        Version = snapshot.Index + 1;
    }

    private void Validate(Event<TAggregate> @event)
    {
        if (@event.AggregateId != Id) throw new Exception($"Invalid aggregate id: {@event.AggregateId}");
        if (@event.Index != Version) throw new Exception($"Invalid aggregate index: {@event.Index}");
    }

    private void Validate(Snapshot<TAggregate> snapshot)
    {
        if (snapshot.AggregateId != Id) throw new Exception($"Invalid aggregate id: {snapshot.AggregateId}");
        if (snapshot.Index != Version) throw new Exception($"Invalid aggregate index: {snapshot.Index}");
    }

    public async Task RehydrateAsync(Snapshot<TAggregate>? snapshot, IAsyncEnumerable<Event<TAggregate>?> events,
        CancellationToken cancellationToken = default)
    {
        if (snapshot is not null) ValidateAndApply(snapshot);
        await foreach (var @event in events.WithCancellation(cancellationToken))
        {
            ValidateAndApply(@event);
        }
    }

    public override void ClearEvents() => _events.Clear();
}