using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wallet.BuildingBlocks.Domain;
using Wallet.Infrastructure.Persistence.EventStore.Entities;

namespace Wallet.Infrastructure.Persistence.EventStore.Repositories;

public class EventStore(EventStoreContext dbContext, IServiceProvider serviceProvider) : IEventStore
{
    public async Task<TAggregate?> RehydrateAsync<TAggregate>(Guid aggregateId, DateTimeOffset? dateTimeOffset = null,
        CancellationToken cancellationToken = default) where TAggregate : Aggregate<TAggregate>, new()
    {
        var snapshot = await GetLatestSnapshotAsync<TAggregate>(aggregateId, dateTimeOffset, cancellationToken);
        var events = GetEventStreamAsync<TAggregate>(aggregateId, dateTimeOffset, snapshot?.Index, cancellationToken);

        var aggregate = new TAggregate() { Id = aggregateId };
        await aggregate.RehydrateAsync(snapshot, events, cancellationToken);

        return aggregate?.Version == 0 ? null : aggregate;
    }

    public async Task PersistAsync<TAggregate>(TAggregate aggregate,
        CancellationToken cancellationToken = default) where TAggregate : Aggregate<TAggregate>, new()
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var @event in aggregate.Events)
        {
            var eventEntity = new EventEntity
            {
                Id = Guid.NewGuid(),
                AggregateId = @event.AggregateId,
                Index = @event.Index,
                AggregateType = @event.AggregateType,
                Type = @event.Type,
                Payload = System.Text.Json.JsonSerializer.Serialize(@event),
                Timestamp = @event.Timestamp,
            };
            await dbContext.Events.AddAsync(eventEntity, cancellationToken);
        }

        aggregate.ClearEvents();

        var snapshotFactories = serviceProvider.GetServices<ISnapshotFactory<TAggregate>>();
        foreach (var snapshotFactory in snapshotFactories.Where(c => c.ShouldCaptureSnapshot(aggregate)))
        {
            var snapshot = snapshotFactory.Create(aggregate);
            var snapshotEntity = new SnapshotEntity()
            {
                Id = Guid.NewGuid(),
                AggregateId = snapshot.AggregateId,
                Version = snapshot.Index,
                State = System.Text.Json.JsonSerializer.Serialize(snapshot),
                Type = snapshot.Type,
                AggregateType = snapshot.AggregateType,
                CreatedAt = snapshot.Timestamp,
            };
            await dbContext.Snapshots.AddAsync(snapshotEntity, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<Snapshot<TAggregate>?> GetLatestSnapshotAsync<TAggregate>(Guid aggregateId,
        DateTimeOffset? currentDateTimeOffset = null, CancellationToken cancellationToken = default)
        where TAggregate : Aggregate<TAggregate>, new()
    {
        Func<SnapshotEntity, bool> searchExpression = (currentDateTimeOffset is null)
            ? s => s.AggregateId == aggregateId
            : s => s.AggregateId == aggregateId && s.CreatedAt <= currentDateTimeOffset;

        var snapshot = await dbContext.Snapshots.Where(searchExpression).AsQueryable().OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync(cancellationToken);

        return snapshot == null
            ? null
            : System.Text.Json.JsonSerializer.Deserialize<Snapshot<TAggregate>>(snapshot.State);
    }

    private async IAsyncEnumerable<Event<TAggregate>?> GetEventStreamAsync<TAggregate>(Guid aggregateId,
        DateTimeOffset? currentDateTimeOffset = null, long? startIndex = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TAggregate : Aggregate<TAggregate>, new()
    {
        Expression<Func<EventEntity, bool>> searchExpression = (currentDateTimeOffset is null)
            ? startIndex is null
                ? e => e.AggregateId == aggregateId
                : e => e.AggregateId == aggregateId && e.Index > startIndex
            : startIndex is null
                ? e => e.AggregateId == aggregateId && e.Timestamp >= currentDateTimeOffset
                : e => e.AggregateId == aggregateId && e.Timestamp >= currentDateTimeOffset && e.Index > startIndex;

        var events = dbContext.Events
            .Where(searchExpression)
            .OrderBy(s => s.Index)
            .AsAsyncEnumerable();

        await foreach (var @event in events)
        {
            yield return System.Text.Json.JsonSerializer.Deserialize<Event<TAggregate>>(@event.Payload);
        }
    }
}