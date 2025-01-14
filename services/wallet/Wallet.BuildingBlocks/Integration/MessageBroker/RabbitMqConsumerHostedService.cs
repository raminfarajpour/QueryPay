using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Wallet.BuildingBlocks.Integration.MessageBroker;

public class ConsumersHostedService(ILogger<ConsumersHostedService> logger, IServiceProvider serviceProvider)
    : BackgroundService
{
    private readonly ILogger _logger = logger;


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        _logger.LogWarning($"{nameof(ConsumersHostedService)} ExecuteAsync started ...");
        try
        {
            using var scope = serviceProvider.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IMessageHandler>();
            var consumer = scope.ServiceProvider.GetRequiredService<IConsumerService>();
            foreach (var handler in handlers)
            {
                await consumer.ConsumeAsync(handler,stoppingToken);
            }
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, $"{nameof(ConsumersHostedService)}  ExecuteAsync has exception : {exp.Message}");
        }
        finally
        {
            _logger.LogWarning($"{nameof(ConsumersHostedService)} ExecuteAsync finished ...");
        }
        
    }


}