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

public class WalletTransactionCreatedIntegrationMessageHandler(
    IServiceProvider serviceProvider,
    IOptions<RabbitMqSetting> rabbitMqOptions,
    ILogger<MessageHandler<WalletTransactionCreatedIntegrationEvent>> logger)
    : MessageHandler<WalletTransactionCreatedIntegrationEvent>(logger)
{
    private readonly ILogger<MessageHandler<WalletTransactionCreatedIntegrationEvent>> _logger = logger;

    public override RabbitMqQueueSetting QueueSetting { get; set; } =
        ((EventBusSetting)rabbitMqOptions.Value).WalletIntegrationEventsExchange.WalletTransactionCreatedEventQueue;


    protected override async Task HandleAsync(WalletTransactionCreatedIntegrationEvent? message,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("{@handler} Message Received: {@message}", nameof(WalletCreatedIntegrationEvent),
            message?.ToJson());

        using var scope = serviceProvider.CreateScope();

        var walletReadModelRepository = scope.ServiceProvider.GetRequiredService<IWalletReadModelRepository>();

        var wallet = await walletReadModelRepository.GetByIdAsync(message!.WalletId, cancellationToken);

        wallet.Transactions.Add((TransactionReadModel)message);
        
        await walletReadModelRepository.ReplaceWalletAsync(wallet, cancellationToken);
    }
}