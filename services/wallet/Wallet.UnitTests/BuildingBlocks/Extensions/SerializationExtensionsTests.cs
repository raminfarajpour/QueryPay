using System;
using FluentAssertions;
using Wallet.BuildingBlocks.Extensions;
using Xunit;

namespace Wallet.UnitTests.BuildingBlocks.Extensions;

public class SerializationExtensionsTests
{
    private class TestClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TestClass Related { get; set; }
    }

    [Fact]
    public void ToJson_ShouldSerializeObject()
    {
        var testObject = new TestClass { Id = 1, Name = "Test" };

        var json = testObject.ToJson();

        json.Should().Contain("\"Id\":1").And.Contain("\"Name\":\"Test\"");
    }

    [Fact]
    public void ToJson_NullObject_ShouldReturnNullString()
    {
        TestClass testObject = null;

        var json = testObject.ToJson();

        json.Should().Be("null");
    }

    [Fact]
    public void ToJson_EmptyObject_ShouldReturnEmptyWithSkippedNullsJsonObject()
    {
        var testObject = new TestClass();

        var json = testObject.ToJson();

        json.Should().Be("{\"Id\":0}");
    }

    [Fact]
    public void ToJson_ObjectWithReferences_ShouldHandleReferenceLoop()
    {
        var testObject1 = new TestClass { Id = 1, Name = "Object1" };
        var testObject2 = new TestClass { Id = 2, Name = "Object2" };

        testObject1.Related = testObject2;
        testObject2.Related = testObject1;

        Action act = () => testObject1.ToJson();

        act.Should().NotThrow();
    }

    [Fact]
    public void ToJson_SpecialCharacters_ShouldEscapeCharactersCorrectly()
    {
        var testObject = new TestClass { Id = 1, Name = "Special \" Characters" };

        var json = testObject.ToJson();

        json.Should().Contain("Special \\u0022 Characters");
    }
}