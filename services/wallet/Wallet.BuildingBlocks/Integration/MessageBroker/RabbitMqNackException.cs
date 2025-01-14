namespace Wallet.BuildingBlocks.Integration.MessageBroker
{
    public class RabbitMqNackException(string message) : Exception(message);
}
