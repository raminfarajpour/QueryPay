using Newtonsoft.Json;
using Wallet.BuildingBlocks.Integration.MessageBroker;

namespace Wallet.BuildingBlocks.Integration;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IIntegrationEvent @event,string exchange, string routingKey, CancellationToken cancellationToken);
}



