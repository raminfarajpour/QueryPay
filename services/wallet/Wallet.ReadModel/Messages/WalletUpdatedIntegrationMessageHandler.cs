using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wallet.Application.IntegrationEvents;
using Wallet.BuildingBlocks.Extensions;
using Wallet.BuildingBlocks.Integration.MessageBroker;
using Wallet.Infrastructure.Integration;
using Wallet.ReadModel.ReadModels;
using Wallet.ReadModel.Repositories;

namespace Wallet.ReadModel.Messages;

public class WalletUpdatedIntegrationMessageHandler(
    IServiceProvider serviceProvider,
    IOptions<RabbitMqSetting> rabbitMqOptions,
    ILogger<MessageHandler<WalletUpdatedIntegrationEvent>> logger)
    : MessageHandler<WalletUpdatedIntegrationEvent>(logger)
{
    private readonly ILogger<MessageHandler<WalletUpdatedIntegrationEvent>> _logger = logger;

    public override RabbitMqQueueSetting QueueSetting { get; set; } =
        ((EventBusSetting)rabbitMqOptions.Value).WalletIntegrationEventsExchange.WalletUpdatedEventQueue;


    protected override async Task HandleAsync(WalletUpdatedIntegrationEvent? message,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("{@handler} Message Received: {@message}", nameof(WalletCreatedIntegrationEvent),
            message?.ToJson());

        using var scope = serviceProvider.CreateScope();

        var walletReadModelRepository = scope.ServiceProvider.GetRequiredService<IWalletReadModelRepository>();

        var currentWallet = await walletReadModelRepository.GetByIdAsync(message!.WalletId, cancellationToken);
        currentWallet.Balance = message.Balance.Amount;
        currentWallet.OverUsedThreshold = message.OverUsedThreshold.Amount;
        currentWallet.Owner = new OwnerReadModel()
        {
            Mobile = message.Owner.Mobile,
            UserId = message.Owner.UserId,
        };
        await walletReadModelRepository.ReplaceWalletAsync(currentWallet, cancellationToken);
    }
}