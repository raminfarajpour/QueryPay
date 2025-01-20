using Wallet.BuildingBlocks.Integration.MessageBroker;

namespace Wallet.Infrastructure.Integration;

public class EventBusSetting:RabbitMqSetting
{
    public WalletIntegrationEventsExchangeSetting WalletIntegrationEventsExchange { get; set; }

    public override List<RabbitMqExchangeSetting> GetExchanges() => [WalletIntegrationEventsExchange];
}