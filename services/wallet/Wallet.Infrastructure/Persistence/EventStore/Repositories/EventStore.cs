using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Wallet.BuildingBlocks.Domain;
using Wallet.Domain.WalletAggregate;
using Wallet.Domain.WalletAggregate.Snapshots;
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
            dynamic dynamicEvent = @event;
            var eventEntity = new EventEntity
            {
                Id = Guid.NewGuid(),
                AggregateId = @event.AggregateId,
                Index = @event.Index,
                AggregateType = @event.AggregateType,
                Type = @event.Type,
                Payload =JsonConvert.SerializeObject(dynamicEvent),
                Timestamp = @event.Timestamp,
            };
            await dbContext.Events.AddAsync(eventEntity, cancellationToken);
        }
        
        var snapshotFactories = serviceProvider.GetServices<ISnapshotFactory<TAggregate>>();
        foreach (var snapshotFactory in snapshotFactories.Where(c => c.ShouldCaptureSnapshot(aggregate)))
        {
            dynamic snapshot = snapshotFactory.Create(aggregate);
            var snapshotEntity = new SnapshotEntity()
            {
                Id = Guid.NewGuid(),
                AggregateId = snapshot.AggregateId,
                Version = snapshot.Index,
                State = JsonConvert.SerializeObject(snapshot),
                Type = snapshot.Type,
                AggregateType = snapshot.AggregateType,
                CreatedAt = snapshot.Timestamp,
            };
            await dbContext.Snapshots.AddAsync(snapshotEntity, cancellationToken);
        }
        
        aggregate.ClearEvents();

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<Snapshot<TAggregate>?> GetLatestSnapshotAsync<TAggregate>(Guid aggregateId,
        DateTimeOffset? currentDateTimeOffset = null, CancellationToken cancellationToken = default)
        where TAggregate : Aggregate<TAggregate>, new()
    {
        var targetDateTimeOffset = currentDateTimeOffset ?? DateTimeOffset.MaxValue;
        Expression<Func<SnapshotEntity, bool>> searchExpression = s => s.AggregateId == aggregateId && s.CreatedAt <= targetDateTimeOffset;

        var snapshot = await dbContext.Snapshots.Where(searchExpression).OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync(cancellationToken);

        return snapshot == null
            ? null
            : JsonConvert.DeserializeObject(snapshot.State,GetSnapshotType(snapshot)) as Snapshot<TAggregate>;
    }

    private async IAsyncEnumerable<Event<TAggregate>?> GetEventStreamAsync<TAggregate>(Guid aggregateId,
        DateTimeOffset? currentDateTimeOffset = null, long? startIndex = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TAggregate : Aggregate<TAggregate>, new()
    {
        var targetDateTimeOffset = currentDateTimeOffset ?? DateTimeOffset.MaxValue;

        Expression<Func<EventEntity, bool>> searchExpression = startIndex is null
                ? e => e.AggregateId == aggregateId && e.Timestamp <= targetDateTimeOffset
                : e => e.AggregateId == aggregateId && e.Timestamp <= targetDateTimeOffset && e.Index > startIndex;

        var events = dbContext.Events
            .Where(searchExpression)
            .OrderBy(s => s.Index);

        await foreach (var @event in events.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            var type = GetEventType(@event);
            yield return JsonConvert.DeserializeObject(@event.Payload, type) as Event<TAggregate>;
        }
    }

    private Type GetEventType(EventEntity @event)
    {
        return @event.Type switch
        {
            "WalletCreatedEvent"=> typeof(WalletCreatedEvent),
            "WalletDepositedEvent"=> typeof(WalletDepositedEvent),
            "WalletWithdrawalEvent"=> typeof(WalletWithdrawalEvent),
            _ => throw new ArgumentOutOfRangeException()
        };

    }
    
    private Type GetSnapshotType(SnapshotEntity snapshot)
    {
        return snapshot.Type switch
        {
            "WalletSnapshot"=> typeof(WalletSnapshot),
            _ => throw new ArgumentOutOfRangeException()
        };

    }
}