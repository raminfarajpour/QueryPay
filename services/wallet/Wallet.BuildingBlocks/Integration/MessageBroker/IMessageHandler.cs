using RabbitMQ.Client.Events;

namespace Wallet.BuildingBlocks.Integration.MessageBroker
{
    public interface IMessageHandler
    {
        public RabbitMqQueueSetting QueueSetting { get; set; }
        Task HandleAsync(object? sender, BasicDeliverEventArgs args, Func<BasicDeliverEventArgs,string,CancellationToken,Task> ackAction, Func<BasicDeliverEventArgs,string, bool,CancellationToken,Task> nAckAction,CancellationToken cancellationToken);
    }
}