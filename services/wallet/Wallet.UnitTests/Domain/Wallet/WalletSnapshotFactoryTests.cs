using FluentAssertions;
using Wallet.Domain.WalletAggregate;
using Wallet.Domain.WalletAggregate.Snapshots;
using Xunit;

namespace Wallet.UnitTests.Domain.Wallet;

public class WalletSnapshotFactoryTests
{
    [Fact]
    public void WalletSnapshotFactory_Should_Have_Correct_Interval()
    {
        // Arrange
        var factory = new WalletSnapshotFactory();

        // Act
        var interval = factory.Interval;

        // Assert
        interval.Should().Be(10);
    }

    [Fact]
    public void WalletSnapshotFactory_Should_Create_Correct_Snapshot()
    {
        // Arrange
        var wallet = new global::Wallet.Domain.WalletAggregate.Wallet();
        wallet.Create(new Money(100), new Money(50), new Owner(1, "1234567890"));
        var factory = new WalletSnapshotFactory();

        // Act
        var snapshotResult = factory.Create(wallet);
        var snapshot =(WalletSnapshot)snapshotResult;
        
        // Assert
        snapshot.Balance.Should().Be(wallet.Balance);
        snapshot.OverUsedThreshold.Should().Be(wallet.OverUsedThreshold);
        snapshot.Owner.Should().Be(wallet.Owner);
        snapshot.CreatedAt.Should().Be(wallet.CreatedAt);
    }
    
   
}