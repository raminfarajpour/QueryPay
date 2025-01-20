using System.Collections.Concurrent;
using Billing.Infrastructure.ExternalServices.SeedWorks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Billing.Infrastructure.SeedWorks.Integration.MessageBroker
{
    public class ConsumerService(IRabbitMqConnectionManager rabbitMqMessageBroker, ILogger<ConsumerService> logger)
        : IConsumerService
    {
        private readonly ConcurrentDictionary<string, AsyncEventingBasicConsumer> _consumers = new();

        private readonly object _lock = new object();
        
        public async Task ConsumeAsync(IMessageHandler handler, CancellationToken cancellationToken)
        {
            await rabbitMqMessageBroker.InitialChannelAsync(cancellationToken);
            
            await rabbitMqMessageBroker.GetChannel().BasicQosAsync(prefetchSize: 0,
                prefetchCount: handler.QueueSetting.PrefetchCount, global: true, cancellationToken: cancellationToken);

            var startedTime = DateTime.Now;
            logger.LogWarning(
                $"RabbitMqMessageBroker StartConsuming add consumer to {handler.QueueSetting.Name} started : {startedTime} ");
            try
            {
                var consumer = await CreateConsumer(handler,cancellationToken);

                lock (_lock)
                {
                    _consumers.TryAdd(handler.QueueSetting.Name, consumer);
                }
            }
            finally
            {
                logger.LogWarning(
                    $"RabbitMqMessageBroker StartConsuming add consumer to {handler.QueueSetting.Name} finished : {DateTime.Now} TotalMilliseconds : {DateTime.Now.Subtract(startedTime).TotalMilliseconds}");
            }
        }

        private async Task<AsyncEventingBasicConsumer> CreateConsumer(IMessageHandler handler,
            CancellationToken cancellationToken)
        {
            logger.LogInformation($"RabbitMqMessageBroker StartConsuming add consumer to {handler.QueueSetting.Name}");
            try
            {
                var consumer = new AsyncEventingBasicConsumer(rabbitMqMessageBroker.GetChannel());
                consumer.RegisteredAsync += (model, args) =>
                {
                    logger.LogInformation($"RabbitMqMessageBroker Registered.");
                    return Task.CompletedTask;
                };
                consumer.ReceivedAsync += (model, args) =>
                {
                    logger.LogInformation($"RabbitMqMessageBroker Received. ");
                    lock (_lock)
                    {
                        return handler.HandleAsync(model, args, Ack, Nack, cancellationToken: cancellationToken);
                    }
                };
                consumer.UnregisteredAsync += (model, args) =>
                {
                    logger.LogInformation($"RabbitMqMessageBroker Unregistered.");
                    return Task.CompletedTask;
                };
                consumer.ShutdownAsync += (model, args) =>
                {
                    logger.LogInformation(
                        $"RabbitMqMessageBroker Shutdown with ShutdownReason:{consumer.ShutdownReason?.ToJson()}.");
                    return Task.CompletedTask;
                };

                await rabbitMqMessageBroker.GetChannel().BasicConsumeAsync(queue: handler.QueueSetting.Name, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

                return consumer;
            }
            finally
            {
                logger.LogWarning(
                    $"RabbitMqMessageBroker StartConsuming add consumer to {handler.QueueSetting.Name} finished.");
            }
        }


        private async Task Ack(BasicDeliverEventArgs args, string queue, CancellationToken cancellationToken)
        {
            if (_consumers.TryGetValue(queue, out var consumer) && consumer.IsRunning)
                await consumer.Channel.BasicAckAsync(args.DeliveryTag, false, cancellationToken: cancellationToken);
        }

        private async Task Nack(BasicDeliverEventArgs args, string queue, bool requeue,
            CancellationToken cancellationToken)
        {
            if (_consumers.TryGetValue(queue, out var consumer) && consumer.IsRunning)
                await consumer.Channel.BasicNackAsync(args.DeliveryTag, false, requeue,
                    cancellationToken: cancellationToken);
        }
    }
}