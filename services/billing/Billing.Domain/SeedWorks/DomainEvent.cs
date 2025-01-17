namespace Billing.Domain.SeedWorks;

public abstract record DomainEvent:IDomainEvent
{
    public DateTime OccurredOn => DateTime.Now;
}