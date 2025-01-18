using RabbitMQ.Client;

namespace Wallet.BuildingBlocks.Integration.MessageBroker;

public interface IRabbitMqConnectionFactory<out TSetting>  where TSetting : RabbitMqSetting,new()
{
    TSetting Settings { get; }
    Task<(IConnection Connection,List<RabbitMqExchangeSetting> ExchangeSettings)> CreateConnectionAsync(CancellationToken cancellationToken);
}