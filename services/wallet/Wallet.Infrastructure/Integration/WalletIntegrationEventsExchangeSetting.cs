using Wallet.BuildingBlocks.Integration.MessageBroker;

namespace Wallet.Infrastructure.Integration;

public class WalletIntegrationEventsExchangeSetting : RabbitMqExchangeSetting
{
    public RabbitMqQueueSetting WalletCreatedEventQueue { get; set; }
    public RabbitMqQueueSetting WalletUpdatedEventQueue { get; set; }
    public RabbitMqQueueSetting WalletTransactionCreatedEventQueue { get; set; }
    public override List<RabbitMqQueueSetting> GetQueues() => [WalletCreatedEventQueue,WalletTransactionCreatedEventQueue,WalletUpdatedEventQueue];
}