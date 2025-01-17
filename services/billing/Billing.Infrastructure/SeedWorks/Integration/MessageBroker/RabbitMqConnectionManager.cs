using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Billing.Infrastructure.SeedWorks.Integration.MessageBroker
{
    public class RabbitMqConnectionManager(
        IRabbitMqConnectionFactory factory,
        ILogger<RabbitMqConnectionManager> logger)
        : IRabbitMqConnectionManager
    {
        private readonly ILogger _logger = logger;

        private IConnection? _connection;
        private IChannel? _channel;


        public async Task InitialChannelAsync(CancellationToken cancellationToken)
        {
            if (_channel is null)
            {
                var createdConnection=await factory.CreateConnectionAsync(cancellationToken);
                _connection = createdConnection.Connection;
                _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
                await DeclareExchangeAndQueuesAsync(createdConnection.ExchangeSettings,cancellationToken);
            }
        }

        public IChannel GetChannel()
        {
            if (_channel is null)
                throw new InvalidOperationException(
                    "Channel has not been initialized. Call InitialChannelAsync first.");
            return _channel!;
        }

        private async Task DeclareExchangeAndQueuesAsync(List<RabbitMqExchangeSetting> exchangeSettings, CancellationToken cancellationToken)
        {

            if (exchangeSettings is null) return;

            foreach (var exchangeSetting in exchangeSettings)
            {
                await _channel.ExchangeDeclareAsync(exchangeSetting.Name, type: exchangeSetting.Type,
                    durable: exchangeSetting.Durable, autoDelete: exchangeSetting.AutoDelete,
                    cancellationToken: cancellationToken);

                var retryExchangeName = $"Retry_{exchangeSetting.Name}";

                await _channel.ExchangeDeclareAsync(retryExchangeName, type: exchangeSetting.Type,
                    durable: exchangeSetting.Durable, autoDelete: exchangeSetting.AutoDelete,
                    cancellationToken: cancellationToken);

                var queueSettings = exchangeSetting.GetQueues();

                if (queueSettings is null) continue;

                foreach (var queue in queueSettings)
                {
                    if(queue is null) continue;
                    
                    var arguments =
                        queue.RetryQueue is not null
                            ? new Dictionary<string, object?>
                            {
                                { "x-dead-letter-exchange", retryExchangeName },
                                { "x-dead-letter-routing-key", queue.RetryQueue.RoutingKey }
                            }
                            : null;

                    await _channel.QueueDeclareAsync(queue.Name,
                        durable: queue.Durable,
                        exclusive: queue.Exclusive,
                        autoDelete: queue.AutoDelete
                        , arguments, cancellationToken: cancellationToken);

                    await _channel.QueueBindAsync(queue: queue.Name, exchange: exchangeSetting.Name,
                        routingKey: queue.RoutingKey, cancellationToken: cancellationToken);

                    if (queue.RetryQueue is null) continue;

                    var ttl = TimeSpan.FromSeconds(queue.Ttl) == TimeSpan.Zero
                        ? TimeSpan.FromSeconds(120)
                        : TimeSpan.FromSeconds(queue.Ttl);

                    await _channel.QueueDeclareAsync(queue.RetryQueue.Name,
                        durable: queue.RetryQueue.Durable,
                        exclusive: queue.RetryQueue.Exclusive,
                        autoDelete: queue.RetryQueue.Exclusive,
                        new Dictionary<string, object?>()
                        {
                            { "x-dead-letter-exchange", exchangeSetting.Name },
                            { "x-message-ttl", (int)ttl.TotalMilliseconds },
                            { "x-dead-letter-routing-key", queue.RoutingKey }
                        }, cancellationToken: cancellationToken);
                    await _channel.QueueBindAsync(queue: queue.RetryQueue.Name, exchange: retryExchangeName,
                        routingKey: queue.RetryQueue.RoutingKey, cancellationToken: cancellationToken);
                }
            }
        }
    }
}