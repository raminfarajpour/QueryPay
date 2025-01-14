namespace Wallet.BuildingBlocks.Integration.MessageBroker
{
    public interface IProducerService
    {
        Task ProduceAsync(byte[] data, string exchange, string routingKey, bool persist = false,CancellationToken cancellationToken=default);
    }
}
