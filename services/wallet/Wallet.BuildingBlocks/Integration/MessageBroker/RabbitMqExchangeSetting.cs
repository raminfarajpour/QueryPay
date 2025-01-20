namespace Wallet.BuildingBlocks.Integration.MessageBroker;

public abstract class RabbitMqExchangeSetting
{
    public string Type { get; set; }
    public string Name { get; set; }
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;

    public abstract List<RabbitMqQueueSetting> GetQueues();


}