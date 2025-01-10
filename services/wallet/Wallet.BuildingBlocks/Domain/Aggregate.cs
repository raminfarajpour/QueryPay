namespace Wallet.BuildingBlocks.Domain;

public abstract class Aggregate
{
    protected Guid Id { get; init; }
    protected string Type { get; init; }
    protected long Version { get; set; }
    internal abstract void ClearEvents();

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
            _previousEvent = _events.LastOrDefault()
        };
    
        ValidateAndApply(e);
        _events.Add(e);
        return e;
    }
    protected abstract void Apply(Event<TAggregate> e);
    
    private void ValidateAndApply(Event<TAggregate> @event)
    {
        ValidateEvent(@event);
        Apply(@event);
        Version++;
    }

    private void ValidateEvent(Event<TAggregate> @event)
    {
        if(@event.AggregateId != Id) throw new Exception($"Invalid aggregate id: {@event.AggregateId}");
        if(@event.Index != Version) throw new Exception($"Invalid aggregate index: {@event.Index}");
    }
}