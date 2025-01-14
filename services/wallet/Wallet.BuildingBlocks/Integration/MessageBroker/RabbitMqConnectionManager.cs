using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Wallet.BuildingBlocks.Integration.MessageBroker
{
    public class RabbitMqConnectionManager<TSetting>(
        ILogger<RabbitMqConnectionManager<TSetting>> logger,
        IOptions<TSetting> rabbitMqSettingOptions)
        : IRabbitMqConnectionManager
        where TSetting : RabbitMqSetting, new()
    {
        private readonly ILogger _logger = logger;
        private readonly TSetting _rabbitMqSetting = rabbitMqSettingOptions.Value;

        private ConnectionFactory? _factory;
        private IConnection? _connection;
        private IChannel? _channel;


        private async Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken)
        {
            _factory ??= new ConnectionFactory();
            _factory.UserName = _rabbitMqSetting.ConnectionSetting.Username;
            _factory.Password = _rabbitMqSetting.ConnectionSetting.Password;
            _factory.HostName = _rabbitMqSetting.ConnectionSetting.HostName;
            _factory.Port = _rabbitMqSetting.ConnectionSetting.Port;
            _factory.VirtualHost = _rabbitMqSetting.ConnectionSetting.VirtualHost;


            _logger.LogInformation($"Connecting to RabbitMQ ...");

            return await _factory.CreateConnectionAsync();
        }


        private async Task DeclareExchangeAndQueuesAsync(CancellationToken cancellationToken)
        {
            var exchangeSettings = _rabbitMqSetting.GetExchanges();

            if (exchangeSettings is null) return;

            foreach (var exchangeSetting in exchangeSettings)
            {
                await _channel.ExchangeDeclareAsync(exchangeSetting.Name, type: exchangeSetting.Type,
                    durable: exchangeSetting.Durable, autoDelete: exchangeSetting.AutoDelete,cancellationToken: cancellationToken);

                var retryExchangeName = $"Retry_{exchangeSetting.Name}";

                await _channel.ExchangeDeclareAsync(retryExchangeName, type: exchangeSetting.Type,
                    durable: exchangeSetting.Durable, autoDelete: exchangeSetting.AutoDelete, cancellationToken: cancellationToken);

                var queueSettings = exchangeSetting.GetQueues();

                if (queueSettings is null) continue;

                foreach (var queue in queueSettings)
                {
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

                    await _channel.QueueBindAsync(queue: queue.Name, exchange: exchangeSetting.Name, routingKey: queue.RoutingKey, cancellationToken: cancellationToken);

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

        public async Task InitialChannelAsync(CancellationToken cancellationToken)
        {
            if(_channel is null)
            {
                _connection = await CreateConnectionAsync(cancellationToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
                await DeclareExchangeAndQueuesAsync(cancellationToken);
            }
        }

        public IChannel GetChannel()
        {
            return _channel;
        }
    }
}