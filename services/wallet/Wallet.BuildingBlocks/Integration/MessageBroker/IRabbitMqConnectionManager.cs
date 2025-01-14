using RabbitMQ.Client;

namespace Wallet.BuildingBlocks.Integration.MessageBroker;

public interface IRabbitMqConnectionManager
{
    Task InitialChannelAsync(CancellationToken cancellationToken);
    IChannel GetChannel();

}