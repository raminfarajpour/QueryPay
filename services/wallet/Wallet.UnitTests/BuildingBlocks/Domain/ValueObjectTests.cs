using FluentAssertions;
using Wallet.BuildingBlocks.Domain;
using Xunit;

namespace Wallet.UnitTests.BuildingBlocks.Domain;

public class ValueObjectTests
{
    private record TestValueObject : ValueObject<TestValueObject>
    {
        public int Value { get; }

        public TestValueObject(int value) => Value = value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    [Fact]
    public void Equals_ShouldReturnTrueForSameValues()
    {
        var obj1 = new TestValueObject(42);
        var obj2 = new TestValueObject(42);

        var result = obj1.Equals(obj2);
        
        result.Should().BeTrue();
        obj1.Should().Be(obj2);
        
    }

    [Fact]
    public void Equals_ShouldReturnFalseForDifferentValues()
    {
        var obj1 = new TestValueObject(42);
        var obj2 = new TestValueObject(43);

        var result = obj1.Equals(obj2);
        
        result.Should().BeFalse();
        obj1.Should().NotBe(obj2);
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistentSameWithValues()
    {
        var obj = new TestValueObject(42);
        var obj2 = new TestValueObject(42);
        
        var result = obj.GetHashCode().Equals(obj2.GetHashCode());
        
        result.Should().BeTrue();
        obj.GetHashCode().Should().Be(obj2.GetHashCode());
    }
}