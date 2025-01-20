using Billing.Application.Commands.ApplyUsage;
using Billing.Domain.Billing;
using Billing.Infrastructure.ExternalServices.SeedWorks;
using Billing.Infrastructure.ExternalServices.WalletService;
using Billing.Infrastructure.Integration;
using Billing.Infrastructure.Persistence;
using Billing.Infrastructure.SeedWorks.Integration;
using Billing.Infrastructure.SeedWorks.Integration.MessageBroker;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Billing.Application.MessageHandlers;

public class UsageMessageHandler(
    IServiceProvider serviceProvider,
    IOptions<EventBusSetting> rabbitMqOptions,
    ILogger<MessageHandler<UsageMessage>> logger)
    : MessageHandler<UsageMessage>(logger)
{
    private readonly ILogger<MessageHandler<UsageMessage>> _logger = logger;

    public override RabbitMqQueueSetting QueueSetting { get; set; } =
        rabbitMqOptions.Value.QueryPayUsageEventsExchange.QueryPayUsageEventsQueue;
    
    protected override async Task HandleAsync(UsageMessage? message, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("{@handler} Message Received: {@message}", nameof(UsageMessage),
                message?.ToJson());

            using var scope = serviceProvider.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var messagePayload = message!.GetPayload();
            var command = new ApplyUsageCommand(message.UserId, messagePayload.Keywords, messagePayload.RowCount);
            await mediator.Send(command, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,"error in consuming event");
            throw;
        }
    }
}