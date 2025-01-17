using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Wallet.Domain.WalletAggregate;
using Wallet.Domain.WalletAggregate.Snapshots;
using Wallet.Infrastructure.Persistence.EventStore.Entities;
using Wallet.IntegrationTests.Fixtures;
using Xunit;

namespace Wallet.IntegrationTests;

public class EventStoreIntegrationTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task RehydrateAsync_ShouldReturnNullWhenNoSnapshotsOrEventsExist()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();

        // Act
        var aggregate = await fixture.EventStore.RehydrateAsync<Domain.WalletAggregate.Wallet>(aggregateId);

        // Assert
        aggregate.Should().BeNull();
    }

    [Fact]
    public async Task RehydrateAsync_ShouldRehydrateFromSnapshotAndEvents()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();

        // Create and save a snapshot
        var snapshot = new SnapshotEntity
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregateId,
            Version = 5,
            Type = "WalletSnapshot",
            State = JsonConvert.SerializeObject(new WalletSnapshot(new Money(500), new Money(500),
                new Owner(1111, "sdfsdfs"), DateTimeOffset.UtcNow.AddHours(-1))
            {
                CreatedAt = DateTimeOffset.UtcNow,
                Index = 5,
                AggregateId = aggregateId,
                AggregateType = nameof(Wallet),
            }),
            AggregateType = nameof(Wallet),
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };
        await fixture.Context.Snapshots.AddAsync(snapshot);
        await fixture.Context.SaveChangesAsync();

        var events = new List<EventEntity>
        {
            new EventEntity
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                Index = 6,
                Type = "WalletDepositedEvent",
                Payload = JsonConvert.SerializeObject(new WalletDepositedEvent(new Money(200),
                    new TransactionInfo("TID", "RID", "DESC"))
                {
                    Index = 6,
                    AggregateId = aggregateId,
                    AggregateType = nameof(Wallet),
                }),
                AggregateType = nameof(Wallet),
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30)
            },
            new EventEntity
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                Index = 7,
                Type = "WalletWithdrawalEvent",
                Payload = JsonConvert.SerializeObject(new WalletWithdrawalEvent(new Money(100),
                    new TransactionInfo("TID", "RID", "DESC"))
                {
                    Index = 7,
                    AggregateId = aggregateId,
                    AggregateType = nameof(Wallet),
                }),
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-15),
                AggregateType = nameof(Wallet)
            }
        };
        await fixture.Context.Events.AddRangeAsync(events);
        await fixture.Context.SaveChangesAsync();

        // Act
        var aggregate = await fixture.EventStore.RehydrateAsync<Domain.WalletAggregate.Wallet>(aggregateId);

        // Assert
        aggregate.Should().NotBeNull();
        aggregate!.Id.Should().Be(aggregateId);
        aggregate.Balance.Should().Be(new Money(600));
        aggregate.Version.Should().Be(8);
    }

    [Fact]
    public async Task RehydrateAsync_ShouldRehydrateFromOnlyEventsWhenNoSnapshot()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();

        // Create and save events
        var events = new List<EventEntity>
        {
            new EventEntity
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                Index = 0,
                Type = "WalletCreatedEvent",
                Payload = JsonConvert.SerializeObject(
                    new WalletCreatedEvent(new Owner(12313213, "123123"), new Money(200), new Money(500))
                    {
                        Index = 0,
                        AggregateId = aggregateId,
                        AggregateType = nameof(Wallet),
                    }),
                AggregateType = nameof(Wallet),
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30)
            },
            new EventEntity
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                Index = 1,
                Type = "WalletDepositedEvent",
                Payload = JsonConvert.SerializeObject(new WalletDepositedEvent(new Money(200),
                    new TransactionInfo("TID", "RID", "DESC"))
                {
                    Index = 1,
                    AggregateId = aggregateId,
                    AggregateType = nameof(Wallet),
                }),
                AggregateType = nameof(Wallet),
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30)
            },
            new EventEntity
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                Index = 2,
                Type = "WalletWithdrawalEvent",
                Payload = JsonConvert.SerializeObject(new WalletWithdrawalEvent(new Money(100),
                    new TransactionInfo("TID", "RID", "DESC"))
                {
                    Index = 2,
                    AggregateId = aggregateId,
                    AggregateType = nameof(Wallet),
                }),
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-15),
                AggregateType = nameof(Wallet)
            }
        };
        await fixture.Context.Events.AddRangeAsync(events);
        await fixture.Context.SaveChangesAsync();

        // Act
        var aggregate = await fixture.EventStore.RehydrateAsync<Domain.WalletAggregate.Wallet>(aggregateId);

        // Assert
        aggregate.Should().NotBeNull();
        aggregate!.Id.Should().Be(aggregateId);
        aggregate.Owner.Should().Be(new Owner(12313213, "123123"));
        aggregate.Balance.Should().Be(new Money(300));
        aggregate.Version.Should().Be(3);
    }

    [Fact]
    public async Task RehydrateAsync_ShouldHandleDeserializationErrorsGracefully()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();

        var invalidEvent = new EventEntity
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregateId,
            Index = 1,
            Type = "WalletDepositedEvent",
            Payload = "Invalid JSON",
            Timestamp = DateTimeOffset.UtcNow,
            AggregateType = nameof(Wallet),
        };
        await fixture.Context.Events.AddAsync(invalidEvent);
        
        // Act
        Func<Task> act = async () =>  await fixture.Context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task RehydrateAsyncShouldRehydrateUpToSpecificTimestamp()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();

        var events = new List<EventEntity>
        {
            new EventEntity
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                Index = 0,
                Type = "WalletCreatedEvent",
                Payload = JsonConvert.SerializeObject(
                    new WalletCreatedEvent(new Owner(12313213, "123123"), new Money(200), new Money(500))
                    {
                        Index = 0,
                        AggregateId = aggregateId,
                        AggregateType = nameof(Wallet),
                    }),
                AggregateType = nameof(Wallet),
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-120)
            },
            new EventEntity
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                Index = 1,
                Type = "WalletDepositedEvent",
                Payload = JsonConvert.SerializeObject(new WalletDepositedEvent(new Money(200),
                    new TransactionInfo("TID", "RID", "DESC"))
                {
                    Index = 1,
                    AggregateId = aggregateId,
                    AggregateType = nameof(Wallet),
                }),
                AggregateType = nameof(Wallet),
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-100)
            },
            new EventEntity
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                Index = 2,
                Type = "WalletWithdrawalEvent",
                Payload = JsonConvert.SerializeObject(new WalletWithdrawalEvent(new Money(100),
                    new TransactionInfo("TID", "RID", "DESC"))
                {
                    Index = 2,
                    AggregateId = aggregateId,
                    AggregateType = nameof(Wallet),
                }),
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-15),
                AggregateType = nameof(Wallet)
            }
        };
        await fixture.Context.Events.AddRangeAsync(events);
        await fixture.Context.SaveChangesAsync();

        // Define a timestamp to rehydrate up to
        var rehydrateUntil = DateTimeOffset.UtcNow.AddMinutes(-100);

        // Act
        var aggregate = await fixture.EventStore.RehydrateAsync<Domain.WalletAggregate.Wallet>(aggregateId, rehydrateUntil);

        // Assert
        aggregate.Should().NotBeNull();
        aggregate!.Id.Should().Be(aggregateId);
        aggregate.Owner.Should().Be(new Owner(12313213, "123123"));
        aggregate.Balance.Should().Be(new Money(400));
        aggregate.Version.Should().Be(2);
    }

    [Fact]
    public async Task PersistAsync_ShouldSaveEventsAndSnapshotsSuccessfully()
    {
        // Arrange
        var aggregate = new Domain.WalletAggregate.Wallet();
        aggregate.Create(new Money(100), new Money(500), new Owner(1111, "11111"));
        aggregate.Deposit(new Money(200), new TransactionInfo("TID1", "RID1", "DESC1"));
        aggregate.Withdraw(new Money(300), new TransactionInfo("TID2", "RID2", "DESC2"));
        aggregate.Deposit(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Withdraw(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Deposit(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Withdraw(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Deposit(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Withdraw(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Deposit(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Withdraw(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Deposit(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Withdraw(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Deposit(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Withdraw(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Deposit(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Withdraw(new Money(25), new TransactionInfo("TID3", "RID3", "DESC3"));
        aggregate.Deposit(new Money(200), new TransactionInfo("TID5", "RID5", "DESC5"));

        // Act
        await fixture.EventStore.PersistAsync(aggregate);

        // Assert
        var savedWallet = await fixture.EventStore.RehydrateAsync<Domain.WalletAggregate.Wallet>(aggregate.Id);
        savedWallet.Should().NotBeNull();
        savedWallet.Version.Should().Be(18);

        var savedSnapshot = await fixture.Context.Snapshots.FirstOrDefaultAsync(s => s.AggregateId == aggregate.Id);
        savedSnapshot.Should().NotBeNull();
        savedSnapshot!.Version.Should().Be(17);
        var deserializedSnapshot = JsonConvert.DeserializeObject<WalletSnapshot>(savedSnapshot.State);
        deserializedSnapshot.Should().NotBeNull();
        deserializedSnapshot!.Balance.Should().Be(new Money(200));
    }

    [Fact]
    public async Task RehydrateAsync_ShouldThrowException_WhenSnapshotTypeIsUnsupported()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();

        var snapshot = new SnapshotEntity
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregateId,
            Version = 1,
            Type = "UnsupportedSnapshot",
            State = $"{{\"AggregateId\":\"{aggregateId}\",\"Balance\":100}}",
            AggregateType = nameof(Wallet),
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };
        await fixture.Context.Snapshots.AddAsync(snapshot);
        await fixture.Context.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await fixture.EventStore.RehydrateAsync<Domain.WalletAggregate.Wallet>(aggregateId);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*");
    }

    [Fact]
    public async Task RehydrateAsync_ShouldThrowException_WhenEventTypeIsUnsupported()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();

        var @event = new EventEntity
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregateId,
            Index = 1,
            Type = "UnsupportedEvent",
            Payload = $"{{\"AggregateId\":\"{aggregateId}\",\"Amount\":100}}",
            Timestamp = DateTimeOffset.UtcNow,
            AggregateType = nameof(Wallet),
        };
        await fixture.Context.Events.AddAsync(@event);
        await fixture.Context.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await fixture.EventStore.RehydrateAsync<Domain.WalletAggregate.Wallet>(aggregateId);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*");
    }

    [Fact]
    public async Task RehydrateAsync_Should_Handle_CancellationToken()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () =>
            await fixture.EventStore.RehydrateAsync<Domain.WalletAggregate.Wallet>(aggregateId, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PersistAsync_Should_Handle_CancellationToken_During_Persistence()
    {
        // Arrange
        var aggregate = new Domain.WalletAggregate.Wallet();
        aggregate.Create(new Money(100), new Money(500), new Owner(1111, "11111"));
        aggregate.Deposit(new Money(200), new TransactionInfo("TID1", "RID1", "DESC1"));

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await fixture.EventStore.PersistAsync(aggregate, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();

        var savedEvents = await fixture.Context.Events
            .Where(e => e.AggregateId == aggregate.Id)
            .ToListAsync(CancellationToken.None);
        savedEvents.Should().BeEmpty();

        var savedSnapshots = await fixture.Context.Snapshots
            .Where(s => s.AggregateId == aggregate.Id)
            .ToListAsync(CancellationToken.None);
        savedSnapshots.Should().BeEmpty();
    }
}