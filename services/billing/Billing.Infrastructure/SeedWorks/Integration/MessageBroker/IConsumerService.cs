namespace Billing.Infrastructure.SeedWorks.Integration.MessageBroker;

public interface IConsumerService
{
    Task ConsumeAsync (IMessageHandler handler ,CancellationToken cancellationToken);
    
   
}