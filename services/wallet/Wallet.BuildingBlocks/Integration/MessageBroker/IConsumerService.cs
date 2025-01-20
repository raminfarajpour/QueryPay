namespace Wallet.BuildingBlocks.Integration.MessageBroker;

public interface IConsumerService
{
    Task ConsumeAsync (IMessageHandler handler ,CancellationToken cancellationToken);
    
   
}