using FluentAssertions;
using Wallet.BuildingBlocks.Domain;
using Xunit;

namespace Wallet.UnitTests.BuildingBlocks.Domain;

public class EntityTests
{
    private class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) => Id = id;
    }

    [Fact]
    public void Equals_ShouldReturnTrueForSameId()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        entity1.Should().Be(entity2);
    }

    [Fact]
    public void Equals_ShouldReturnFalseForDifferentIds()
    {
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        entity1.Should().NotBe(entity2);
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistentWithId()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);

        entity.GetHashCode().Should().Be(id.GetHashCode());
    }
}