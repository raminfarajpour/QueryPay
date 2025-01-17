namespace Billing.Domain.SeedWorks;
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}