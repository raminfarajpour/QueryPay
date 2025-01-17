using Billing.Infrastructure.SeedWorks.Integration.MessageBroker;

namespace Billing.Infrastructure.Integration;

// const (
//     exchange        = "query_pay_usage_events_exchange"
// routingKey      = "ue"
// queue           = "query_pay_usage_events"
// retryExchange   = "retry_query_pay_usage_events_exchange"
// retryRoutingKey = "rue"
// retryQueue      = "retry_query_pay_usage_events"
//     )

public class QueryPayUsageEventsExchangeSetting : RabbitMqExchangeSetting
{
    public RabbitMqQueueSetting QueryPayUsageEventsQueue { get; set; }
    public override List<RabbitMqQueueSetting> GetQueues() => [QueryPayUsageEventsQueue];
}