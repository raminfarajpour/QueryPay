using Wallet.BuildingBlocks.Domain;

namespace Wallet.Infrastructure.Persistence.EventStore.Repositories;

public interface IEventStore
{
    public Task<TAggregate?> RehydrateAsync<TAggregate>(Guid aggregateId, DateTimeOffset? dateTimeOffset = null,
        CancellationToken cancellationToken = default) where TAggregate : Aggregate<TAggregate>, new();

    Task PersistAsync<TAggregate>(TAggregate aggregate,
        CancellationToken cancellationToken = default) where TAggregate : Aggregate<TAggregate>, new();
}