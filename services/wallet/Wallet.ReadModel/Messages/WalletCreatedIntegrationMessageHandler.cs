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

public class WalletCreatedIntegrationMessageHandler(
    IServiceProvider serviceProvider,
    IOptions<RabbitMqSetting> rabbitMqOptions,
    ILogger<MessageHandler<WalletCreatedIntegrationEvent>> logger)
    : MessageHandler<WalletCreatedIntegrationEvent>(logger)
{
    private readonly ILogger<MessageHandler<WalletCreatedIntegrationEvent>> _logger = logger;

    public override RabbitMqQueueSetting QueueSetting { get; set; } =
        ((EventBusSetting)rabbitMqOptions.Value).WalletIntegrationEventsExchange.WalletCreatedEventQueue;


    protected override async Task HandleAsync(WalletCreatedIntegrationEvent? message,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("{@handler} Message Received: {@message}", nameof(WalletCreatedIntegrationEvent),
            message?.ToJson());

        using var scope = serviceProvider.CreateScope();

        var walletReadModelRepository = scope.ServiceProvider.GetRequiredService<IWalletReadModelRepository>();

        var readModel = (WalletReadModel)message!;
        await walletReadModelRepository.SaveAsync(readModel, cancellationToken);
    }
}