using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Wallet.BuildingBlocks.Integration.MessageBroker;

public class RabbitMqConnectionFactory<TSetting>(
    ILogger<RabbitMqConnectionFactory<TSetting>> logger,
    IOptions<TSetting>? rabbitOptions) : IRabbitMqConnectionFactory<TSetting>  where TSetting : RabbitMqSetting,new()
{
    public  TSetting Settings =>
        rabbitOptions?.Value ?? throw new ArgumentNullException(nameof(rabbitOptions));
    

    public async Task<(IConnection,List<RabbitMqExchangeSetting>)> CreateConnectionAsync(
        CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            UserName = Settings.ConnectionSetting.Username,
            Password = Settings.ConnectionSetting.Password,
            HostName = Settings.ConnectionSetting.HostName,
            Port = Settings.ConnectionSetting.Port,
            VirtualHost = Settings.ConnectionSetting.VirtualHost
        };

        logger.LogInformation($"Connecting to RabbitMQ ...");

        var connection= await factory.CreateConnectionAsync(cancellationToken);
        return (connection, Settings.GetExchanges());
    }
}