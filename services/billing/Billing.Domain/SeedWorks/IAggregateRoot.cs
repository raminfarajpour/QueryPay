namespace Billing.Domain.SeedWorks;

public interface IAggregateRoot
{
    IReadOnlyCollection<DomainEvent> Events { get; }
    void ClearEvents();
    void RemoveEvent(DomainEvent domainEvent);
}
