using RabbitMQ.Client;

namespace Billing.Infrastructure.SeedWorks.Integration.MessageBroker;

public interface IRabbitMqConnectionFactory
{
    Task<(IConnection Connection,List<RabbitMqExchangeSetting> ExchangeSettings)> CreateConnectionAsync(CancellationToken cancellationToken);
}