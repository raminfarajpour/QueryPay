namespace Billing.Infrastructure.SeedWorks.Integration.MessageBroker
{
    public class RabbitMqNackException(string message) : Exception(message);
}
