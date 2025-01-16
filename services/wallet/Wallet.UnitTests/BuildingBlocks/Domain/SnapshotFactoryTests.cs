using FluentAssertions;
using Wallet.BuildingBlocks.Domain;
using Xunit;

namespace Wallet.UnitTests.BuildingBlocks.Domain;

public class SnapshotFactoryTests
{
    private class TestAggregate : Aggregate<TestAggregate>
    {
        protected override void Apply(Event<TestAggregate> @event) { }
        protected override void Apply(Snapshot<TestAggregate> snapshot) { }
    }

    private record TestSnapshot : Snapshot<TestAggregate>
    {
        public TestSnapshot() { }
    }

    private record TestSnapshotWithShouldCapture : ISnapshotFactory<TestAggregate>
    {
        public TestSnapshotWithShouldCapture() { }
        public int Interval => 5;

        public bool ShouldCaptureSnapshot(TestAggregate aggregate) => true;
        public Snapshot<TestAggregate> Create(TestAggregate aggregate) => null!;
    }
    private record TestEvent : Event<TestAggregate> { }

    private class TestSnapshotFactory : SnapshotFactory<TestAggregate, TestSnapshot>
    {
        public override int Interval => 5;

        protected override TestSnapshot CreateSnapshot(TestAggregate aggregate)
        {
            return new TestSnapshot();
        }
    }

    [Fact]
    public void ShouldCaptureSnapshot_ShouldReturnTrueForInterval()
    {
        var factory = new TestSnapshotFactory();
        var aggregate = new TestAggregate();

        aggregate.Apply(new TestEvent());
        aggregate.Apply(new TestEvent());

        factory.ShouldCaptureSnapshot(aggregate).Should().BeFalse();

        aggregate.Apply(new TestEvent());
        aggregate.Apply(new TestEvent());
        aggregate.Apply(new TestEvent());

        factory.ShouldCaptureSnapshot(aggregate).Should().BeTrue();
    }
    
    [Fact]
    public void ShouldCaptureSnapshot_ShouldReturnExpectedValue()
    {
        var factory = new TestSnapshotWithShouldCapture();
        var aggregate = new TestAggregate();

        factory.ShouldCaptureSnapshot(aggregate).Should().BeTrue();
    }
}