using Billing.Domain.Billing;
using Billing.Infrastructure.ExternalServices.SeedWorks;
using Billing.Infrastructure.ExternalServices.WalletService;
using Billing.Infrastructure.Integration;
using Billing.Infrastructure.Persistence;
using Billing.Infrastructure.SeedWorks.Integration;
using Billing.Infrastructure.SeedWorks.Integration.MessageBroker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Billing.Application.MessageHandlers;

public class UsageMessageHandler(
    IServiceProvider serviceProvider,
    IOptions<RabbitMqSetting> rabbitMqOptions,
    ILogger<MessageHandler<UsageMessage>> logger)
    : MessageHandler<UsageMessage>(logger)
{
    private readonly ILogger<MessageHandler<UsageMessage>> _logger = logger;

    public override RabbitMqQueueSetting QueueSetting { get; set; } =
        ((EventBusSetting)rabbitMqOptions.Value).QueryPayUsageEventsExchange.QueryPayUsageEventsQueue;

    const long UserId = 12345678L; //for test

    protected override async Task HandleAsync(UsageMessage? message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{@handler} Message Received: {@message}", nameof(UsageMessage),
            message?.ToJson());

        using var scope = serviceProvider.CreateScope();

        var billingRepository = scope.ServiceProvider.GetRequiredService<IBillingRepository>();
        var walletProvider = scope.ServiceProvider.GetRequiredService<IWalletProviderService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        var billing = billingRepository.GetByUserIdAsync(UserId, cancellationToken);
            var readModel = (WalletReadModel)message!;
        await walletReadModelRepository.SaveAsync(readModel, cancellationToken);
    }
}