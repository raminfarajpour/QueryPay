
namespace Billing.Domain.SeedWorks;
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    public IReadOnlyCollection<DomainEvent> Events => [.. _events];

    private readonly List<DomainEvent> _events = [];

    protected AggregateRoot() {}
    protected AggregateRoot(TId id) 
    {
        Id = id;    
    }

    public void ClearEvents() => _events.Clear();

    protected void RaiseEvent<TDomainEvent>(TDomainEvent @event)
        where TDomainEvent : DomainEvent => _events.Add(@event);
    
    public void RemoveEvent(DomainEvent domainEvent)
    {
        _events.Remove(domainEvent);
    }
}