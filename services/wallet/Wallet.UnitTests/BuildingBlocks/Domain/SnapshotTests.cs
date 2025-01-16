using FluentAssertions;
using Wallet.BuildingBlocks.Domain;
using Xunit;

namespace Wallet.UnitTests.BuildingBlocks.Domain;

public class SnapshotTests
{
    private record TestSnapshot : Snapshot<TestAggregate>
    {
        public TestSnapshot()
        {
            AggregateId = Guid.NewGuid();
            AggregateType = nameof(TestAggregate);
            Index = 1;
        }
    }

    private class TestAggregate : Aggregate<TestAggregate>
    {
        protected override void Apply(Event<TestAggregate> @event) { }
        protected override void Apply(Snapshot<TestAggregate> snapshot) { }
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        var snapshot = new TestSnapshot();

        snapshot.Type.Should().Be(nameof(TestSnapshot));
        snapshot.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        snapshot.Index.Should().Be(1);
    }
}