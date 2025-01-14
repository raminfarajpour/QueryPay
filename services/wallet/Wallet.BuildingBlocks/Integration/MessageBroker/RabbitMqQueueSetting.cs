namespace Wallet.BuildingBlocks.Integration.MessageBroker;

public class RabbitMqQueueSetting
{
    public string Name { get; set; }
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public bool Exclusive { get; set; } = false;
    public int Ttl { get; set; }
    public int ExpireTime { get; set; }
    public string RoutingKey { get; set; }
    public ushort PrefetchCount { get; set; }

    public RabbitMqQueueSetting? RetryQueue { get; set; }
}