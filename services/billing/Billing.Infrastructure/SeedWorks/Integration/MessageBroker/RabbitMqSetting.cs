namespace Billing.Infrastructure.SeedWorks.Integration.MessageBroker;

public abstract class RabbitMqSetting
{
    public RabbitMqConnectionSetting ConnectionSetting { get; set; }
    
    public abstract List<RabbitMqExchangeSetting> GetExchanges();

}