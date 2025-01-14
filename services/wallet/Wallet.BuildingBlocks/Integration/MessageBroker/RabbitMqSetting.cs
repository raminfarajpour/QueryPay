namespace Wallet.BuildingBlocks.Integration.MessageBroker;

public abstract class RabbitMqSetting
{
    public RabbitMqConnectionSetting ConnectionSetting { get; set; }
    
    public abstract List<RabbitMqExchangeSetting> GetExchanges();

}