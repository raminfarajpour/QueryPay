using FluentAssertions;
using Wallet.BuildingBlocks.Domain;
using Xunit;

namespace Wallet.UnitTests.BuildingBlocks.Domain;

public class EventTests
{
    private record TestEvent : Event { }

    [Fact]
    public void Constructor_ShouldInitializeTypeAndTimestamp()
    {
        var @event = new TestEvent();

        @event.Type.Should().Be(nameof(TestEvent));
        @event.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}