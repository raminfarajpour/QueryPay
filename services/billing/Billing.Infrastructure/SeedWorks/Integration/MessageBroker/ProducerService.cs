﻿using RabbitMQ.Client;

namespace Billing.Infrastructure.SeedWorks.Integration.MessageBroker
{
    public class ProducerService(IRabbitMqConnectionManager messageBroker) : IProducerService
    {
        public async Task ProduceAsync(byte[] data, string exchange, string routingKey, bool persist = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            await messageBroker.InitialChannelAsync(cancellationToken);
            
            var basicProperties = new BasicProperties
            {
                Persistent = persist
            };

            await messageBroker.GetChannel().BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: basicProperties,
                body: data, cancellationToken: cancellationToken);


        }
    }
}