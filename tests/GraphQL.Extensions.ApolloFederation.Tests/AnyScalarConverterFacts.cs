using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.ValueSerialization;
using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

public class AnyScalarConverterFacts
{
    public AnyScalarConverterFacts()
    {
        Sut = new AnyScalarConverter();
    }

    protected AnyScalarConverter Sut { get; }

    [Theory]
    [InlineData(null)]
    [InlineData("string value")]
    [InlineData(42)]
    [InlineData(3.14)]
    [InlineData(true)]
    [InlineData(false)]
    public void Serialize_ReturnsInputValue(object input)
    {
        // Given
        // When
        var result = Sut.Serialize(input);

        // Then
        Assert.Equal(input, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("string value")]
    [InlineData(42)]
    [InlineData(3.14)]
    [InlineData(true)]
    [InlineData(false)]
    public void ParseValue_ReturnsInputValue(object input)
    {
        // Given
        // When
        var result = Sut.ParseValue(input);

        // Then
        Assert.Equal(input, result);
    }

    [Fact]
    public void ParseValue_ComplexObject_ReturnsInputValue()
    {
        // Given
        var complexObject = new Dictionary<string, object>
        {
            ["key1"] = "value1",
            ["key2"] = 42,
            ["key3"] = new Dictionary<string, object>
            {
                ["nested"] = "value"
            }
        };

        // When
        var result = Sut.ParseValue(complexObject);

        // Then
        Assert.Equal(complexObject, result);
    }

    [Fact]
    public void ParseValue_Array_ReturnsInputValue()
    {
        // Given
        var array = new object[] { "value1", 42, true, null };

        // When
        var result = Sut.ParseValue(array);

        // Then
        Assert.Equal(array, result);
    }

    [Theory]
    [InlineData("string value")]
    [InlineData(42)]
    [InlineData(3.14)]
    [InlineData(true)]
    [InlineData(false)]
    public void SerializeLiteral_ReturnsNullValue(object input)
    {
        // Given
        // When
        var result = Sut.SerializeLiteral(input);

        // Then
        Assert.IsType<NullValue>(result);
    }

    [Fact]
    public void SerializeLiteral_NullInput_ReturnsNullValue()
    {
        // Given
        // When
        var result = Sut.SerializeLiteral(null);

        // Then
        Assert.IsType<NullValue>(result);
    }

    [Fact]
    public void ParseLiteral_StringValue_ReturnsStringValue()
    {
        // Given
        var stringValue = new StringValue("test");

        // When
        var result = Sut.ParseLiteral(stringValue);

        // Then
        Assert.Equal(stringValue, result);
    }

    [Fact]
    public void ParseLiteral_IntValue_ReturnsIntValue()
    {
        // Given
        var intValue = new IntValue(42);

        // When
        var result = Sut.ParseLiteral(intValue);

        // Then
        Assert.Equal(intValue, result);
    }

    [Fact]
    public void ParseLiteral_BooleanValue_ReturnsBooleanValue()
    {
        // Given
        var boolValue = new BooleanValue(true);

        // When
        var result = Sut.ParseLiteral(boolValue);

        // Then
        Assert.Equal(boolValue, result);
    }

    [Fact]
    public void ParseLiteral_NullValue_ReturnsNullValue()
    {
        // Given
        var nullValue = new NullValue();

        // When
        var result = Sut.ParseLiteral(nullValue);

        // Then
        Assert.Equal(nullValue, result);
    }

    [Fact]
    public void ParseLiteral_ObjectValue_ReturnsObjectValue()
    {
        // Given
        var objectValue = new ObjectValue(new List<ObjectField>
        {
            new("key1", new StringValue("value1")),
            new("key2", new IntValue(42))
        });

        // When
        var result = Sut.ParseLiteral(objectValue);

        // Then
        Assert.Equal(objectValue, result);
    }

    [Fact]
    public void ParseLiteral_ListValue_ReturnsListValue()
    {
        // Given
        var listValue = new ListValue(new List<ValueBase>
        {
            new StringValue("value1"),
            new IntValue(42),
            new BooleanValue(true)
        });

        // When
        var result = Sut.ParseLiteral(listValue);

        // Then
        Assert.Equal(listValue, result);
    }
}