using RabbitMQ.Client;

namespace Billing.Infrastructure.SeedWorks.Integration.MessageBroker;

public interface IRabbitMqConnectionFactory<TSetting> where TSetting : RabbitMqSetting,new()
{
    TSetting Settings { get; }
    Task<(IConnection Connection, List<RabbitMqExchangeSetting> ExchangeSettings)> CreateConnectionAsync(
        CancellationToken cancellationToken);
}