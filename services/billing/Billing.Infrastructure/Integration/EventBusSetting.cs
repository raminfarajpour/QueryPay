using Billing.Infrastructure.SeedWorks.Integration.MessageBroker;

namespace Billing.Infrastructure.Integration;

public class EventBusSetting:RabbitMqSetting
{
    public QueryPayUsageEventsExchangeSetting QueryPayUsageEventsExchange{ get; set; }

    public RabbitMqConnectionSetting ConnectionSetting { get; set; }
    public override List<RabbitMqExchangeSetting> GetExchanges() => [QueryPayUsageEventsExchange];
}