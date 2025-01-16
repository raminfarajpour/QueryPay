using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Wallet.BuildingBlocks.Integration.MessageBroker;

public class RabbitMqConnectionFactory(
    ILogger<RabbitMqConnectionFactory> logger,
    IOptions<RabbitMqSetting>? rabbitOptions) : IRabbitMqConnectionFactory
{
    private readonly RabbitMqSetting _rabbitMqSetting =
        rabbitOptions?.Value ?? throw new ArgumentNullException(nameof(rabbitOptions));

    public async Task<(IConnection,List<RabbitMqExchangeSetting>)> CreateConnectionAsync(
        CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            UserName = _rabbitMqSetting.ConnectionSetting.Username,
            Password = _rabbitMqSetting.ConnectionSetting.Password,
            HostName = _rabbitMqSetting.ConnectionSetting.HostName,
            Port = _rabbitMqSetting.ConnectionSetting.Port,
            VirtualHost = _rabbitMqSetting.ConnectionSetting.VirtualHost
        };

        logger.LogInformation($"Connecting to RabbitMQ ...");

        var connection= await factory.CreateConnectionAsync(cancellationToken);
        return (connection, _rabbitMqSetting.GetExchanges());
    }
}