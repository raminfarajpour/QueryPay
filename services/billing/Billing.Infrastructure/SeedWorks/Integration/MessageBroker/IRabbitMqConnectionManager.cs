using RabbitMQ.Client;

namespace Billing.Infrastructure.SeedWorks.Integration.MessageBroker;

public interface IRabbitMqConnectionManager
{
    Task InitialChannelAsync(CancellationToken cancellationToken);
    IChannel GetChannel();

}