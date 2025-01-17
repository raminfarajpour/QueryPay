namespace Billing.Infrastructure.SeedWorks.Integration;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IIntegrationEvent @event,string exchange, string routingKey, CancellationToken cancellationToken);
}



