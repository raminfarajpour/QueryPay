using RabbitMQ.Client;

namespace Wallet.BuildingBlocks.Integration.MessageBroker;

public interface IRabbitMqConnectionFactory
{
    Task<(IConnection Connection,List<RabbitMqExchangeSetting> ExchangeSettings)> CreateConnectionAsync(CancellationToken cancellationToken);
}