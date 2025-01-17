using System.Text;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wallet.Application.IntegrationEvents;
using Wallet.BuildingBlocks.Integration;
using Wallet.BuildingBlocks.Integration.MessageBroker;
using Wallet.Domain.WalletAggregate;
using Wallet.Infrastructure.Integration;
using Wallet.Infrastructure.Persistence.EventStore.Repositories;

namespace Wallet.Application.Commands.CreateWallet;

public class CreateWalletCommandHandler(
    IEventStore eventStore,
    IProducerService eventPublisher,
    IOptions<EventBusSetting> eventBusSettingOptions)
    : IRequestHandler<CreateWalletCommand>
{
    private EventBusSetting EventBusSetting { get; } = eventBusSettingOptions.Value ??
                                                       throw new ArgumentNullException(nameof(eventBusSettingOptions));

    public async Task Handle(CreateWalletCommand command, CancellationToken cancellationToken)
    {
        var wallet = new Domain.WalletAggregate.Wallet();
        wallet.Create(
            initialBalance: new Money(command.InitialBalance),
            overUsedThreshold: new Money(command.OverUsedThreshold),
            new Owner(command.OwnerUserId, command.OwnerMobile)
        );

        await eventStore.PersistAsync(wallet, cancellationToken);

        await PublishIntegrationEvents(cancellationToken, wallet);

       
    }

    private async Task PublishIntegrationEvents(CancellationToken cancellationToken, Domain.WalletAggregate.Wallet? wallet)
    {
        var walletCreatedIntegrationEvent = new WalletCreatedIntegrationEvent(wallet!.Id, wallet!.Balance,
            wallet.OverUsedThreshold, wallet.Owner, wallet.CreatedAt);
        
        await PublishAsync(walletCreatedIntegrationEvent,
            EventBusSetting.WalletIntegrationEventsExchange.WalletCreatedEventQueue.RoutingKey, cancellationToken);
        
        var transactionIntegrationEventsPublishTasks =
            wallet?.Transactions.Select(transaction =>
                PublishAsync(new WalletTransactionCreatedIntegrationEvent(wallet.Id, transaction),
                    EventBusSetting.WalletIntegrationEventsExchange.WalletTransactionCreatedEventQueue.RoutingKey,
                    cancellationToken));

        if (transactionIntegrationEventsPublishTasks != null)
            await Task.WhenAll(transactionIntegrationEventsPublishTasks);
    }

    private async Task PublishAsync(IIntegrationEvent @event, string routingKey, CancellationToken cancellationToken)
    {
        var message = JsonConvert.SerializeObject(@event);
        var data = Encoding.UTF8.GetBytes(message);


        await eventPublisher.ProduceAsync(data, EventBusSetting.WalletIntegrationEventsExchange.Name,
            routingKey, persist: true,
            cancellationToken: cancellationToken);
    }
}