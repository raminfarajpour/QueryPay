using FluentAssertions;
using Wallet.BuildingBlocks.Domain;
using Xunit;

namespace Wallet.UnitTests.BuildingBlocks.Domain;

public class AggregateTests
{
    private record TestEvent : Event<TestAggregate>
    {
    }

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
        protected override void Apply(Event<TestAggregate> @event)
        {
        }

        protected override void Apply(Snapshot<TestAggregate> snapshot)
        {
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeIdAndType()
    {
        var aggregate = new TestAggregate();

        aggregate.Id.Should().NotBeEmpty();
        aggregate.Type.Should().Be(nameof(TestAggregate));
    }

    [Fact]
    public void Apply_ShouldAddEventAndIncreaseVersion()
    {
        var aggregate = new TestAggregate();
        var testEvent = new TestEvent();

        var appliedEvent = aggregate.Apply(testEvent);

        appliedEvent.Should().NotBeNull();
        appliedEvent.Index.Should().Be(0);
        aggregate.Version.Should().Be(1);
        aggregate.Events.Should().ContainSingle().Which.Should().Be(appliedEvent);
    }

    [Fact]
    public void Apply_NullEvent_ShouldThrowArgumentNullException()
    {
        var aggregate = new TestAggregate();

        Action act = () => aggregate.Apply<TestEvent>(null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("e");
    }

    [Fact]
    public void ClearEvents_ShouldClearAllEvents()
    {
        var aggregate = new TestAggregate();
        aggregate.Apply(new TestEvent());

        aggregate.ClearEvents();

        aggregate.Events.Should().BeEmpty();
    }

    [Fact]
    public async Task RehydrateAsync_ShouldApplyEventsAndSnapshots()
    {
        var aggregate = new TestAggregate();
        var snapshot = new TestSnapshot { AggregateId = aggregate.Id, Index = 0 };
        var testEvent = new TestEvent { AggregateId = aggregate.Id, Index = 1 };

        var events = CreateAsyncEnumerable(new List<TestEvent> { testEvent });

        await aggregate.RehydrateAsync(snapshot, events);

        aggregate.Version.Should().Be(2);
    }

    [Fact]
    public void ValidateAndApply_InvalidAggregateId_ShouldNotThrowException()
    {
        var aggregate = new TestAggregate();
        var testEvent = new TestEvent { AggregateId = Guid.NewGuid(), Index = 0 };

        Action act = () => aggregate.Apply(testEvent);

        act.Should().NotThrow();
    }


    private static async IAsyncEnumerable<T> CreateAsyncEnumerable<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
            await Task.Yield(); 
        }
    }
}