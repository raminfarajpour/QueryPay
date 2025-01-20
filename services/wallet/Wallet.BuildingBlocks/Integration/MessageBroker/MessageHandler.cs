using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace Wallet.BuildingBlocks.Integration.MessageBroker;

public abstract class MessageHandler<T>(ILogger<MessageHandler<T>> logger) : IMessageHandler where T : IIntegrationEvent
{
    public abstract RabbitMqQueueSetting QueueSetting { get; set; }
    public async  Task HandleAsync(object? sender, BasicDeliverEventArgs args, Func<BasicDeliverEventArgs, string, CancellationToken, Task> ackAction, Func<BasicDeliverEventArgs, string, bool, CancellationToken, Task> nAckAction,CancellationToken cancellationToken )
    {
        try
        {
            var message = GetMessage(args);
            await HandleAsync(message,cancellationToken);
            await ackAction.Invoke(args, QueueSetting.Name,cancellationToken);
        }
        catch (Exception)
        {
           await  nAckAction.Invoke(args, QueueSetting.Name, false,cancellationToken);
        }
    }
    

    protected abstract Task HandleAsync(T? message,CancellationToken cancellationToken);


    private T? GetMessage(BasicDeliverEventArgs args)
    {
        var message = Encoding.UTF8.GetString((byte[])args.Body.ToArray());
        return JsonConvert.DeserializeObject<T>(message);
    }
}