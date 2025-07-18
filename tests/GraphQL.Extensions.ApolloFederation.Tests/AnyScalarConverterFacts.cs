using System;
using System.Collections.Generic;
using Tanka.GraphQL.Extensions.ApolloFederation;
using Tanka.GraphQL.Language.Nodes;
using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

public class AnyScalarConverterFacts
{
    [Fact]
    public void Serialize_ShouldReturnInputValueUnchanged()
    {
        // Given
        var converter = new AnyScalarConverter();
        var value = "test string";
        
        // When
        var result = converter.Serialize(value);
        
        // Then
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_ShouldHandleNullValue()
    {
        // Given
        var converter = new AnyScalarConverter();
        
        // When
        var result = converter.Serialize(null);
        
        // Then
        Assert.Null(result);
    }

    [Fact]
    public void Serialize_ShouldHandleComplexObject()
    {
        // Given
        var converter = new AnyScalarConverter();
        var value = new { Name = "John", Age = 30 };
        
        // When
        var result = converter.Serialize(value);
        
        // Then
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_ShouldHandleCollection()
    {
        // Given
        var converter = new AnyScalarConverter();
        var value = new List<string> { "item1", "item2", "item3" };
        
        // When
        var result = converter.Serialize(value);
        
        // Then
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_ShouldHandleDictionary()
    {
        // Given
        var converter = new AnyScalarConverter();
        var value = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 42 },
            { "key3", true }
        };
        
        // When
        var result = converter.Serialize(value);
        
        // Then
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_ShouldHandlePrimitiveTypes()
    {
        // Given
        var converter = new AnyScalarConverter();
        var testCases = new object[]
        {
            "string",
            42,
            3.14,
            true,
            DateTime.Now,
            Guid.NewGuid()
        };
        
        // When & Then
        foreach (var testCase in testCases)
        {
            var result = converter.Serialize(testCase);
            Assert.Equal(testCase, result);
        }
    }

    [Fact]
    public void SerializeLiteral_ShouldAlwaysReturnNullValue()
    {
        // Given
        var converter = new AnyScalarConverter();
        
        // When
        var result = converter.SerializeLiteral("any value");
        
        // Then
        Assert.IsType<NullValue>(result);
    }

    [Fact]
    public void SerializeLiteral_ShouldReturnNullValueForNullInput()
    {
        // Given
        var converter = new AnyScalarConverter();
        
        // When
        var result = converter.SerializeLiteral(null);
        
        // Then
        Assert.IsType<NullValue>(result);
    }

    [Fact]
    public void SerializeLiteral_ShouldReturnNullValueForComplexObject()
    {
        // Given
        var converter = new AnyScalarConverter();
        var complexObject = new { Name = "John", Age = 30 };
        
        // When
        var result = converter.SerializeLiteral(complexObject);
        
        // Then
        Assert.IsType<NullValue>(result);
    }

    [Fact]
    public void ParseValue_ShouldReturnInputValueUnchanged()
    {
        // Given
        var converter = new AnyScalarConverter();
        var value = "test input";
        
        // When
        var result = converter.ParseValue(value);
        
        // Then
        Assert.Equal(value, result);
    }

    [Fact]
    public void ParseValue_ShouldHandleNullInput()
    {
        // Given
        var converter = new AnyScalarConverter();
        
        // When
        var result = converter.ParseValue(null);
        
        // Then
        Assert.Null(result);
    }

    [Fact]
    public void ParseValue_ShouldHandleComplexInput()
    {
        // Given
        var converter = new AnyScalarConverter();
        var input = new Dictionary<string, object>
        {
            { "field1", "value1" },
            { "field2", 123 }
        };
        
        // When
        var result = converter.ParseValue(input);
        
        // Then
        Assert.Equal(input, result);
    }

    [Fact]
    public void ParseValue_ShouldHandleArrayInput()
    {
        // Given
        var converter = new AnyScalarConverter();
        var input = new object[] { "item1", 42, true };
        
        // When
        var result = converter.ParseValue(input);
        
        // Then
        Assert.Equal(input, result);
    }

    [Fact]
    public void ParseLiteral_ShouldReturnLiteralValueUnchanged()
    {
        // Given
        var converter = new AnyScalarConverter();
        var literal = new StringValue { Value = "test literal" };
        
        // When
        var result = converter.ParseLiteral(literal);
        
        // Then
        Assert.Equal(literal, result);
    }

    [Fact]
    public void ParseLiteral_ShouldHandleNullValue()
    {
        // Given
        var converter = new AnyScalarConverter();
        var literal = new NullValue();
        
        // When
        var result = converter.ParseLiteral(literal);
        
        // Then
        Assert.Equal(literal, result);
    }

    [Fact]
    public void ParseLiteral_ShouldHandleIntValue()
    {
        // Given
        var converter = new AnyScalarConverter();
        var literal = new IntValue { Value = 42 };
        
        // When
        var result = converter.ParseLiteral(literal);
        
        // Then
        Assert.Equal(literal, result);
    }

    [Fact]
    public void ParseLiteral_ShouldHandleFloatValue()
    {
        // Given
        var converter = new AnyScalarConverter();
        var literal = new FloatValue { Value = 3.14 };
        
        // When
        var result = converter.ParseLiteral(literal);
        
        // Then
        Assert.Equal(literal, result);
    }

    [Fact]
    public void ParseLiteral_ShouldHandleBooleanValue()
    {
        // Given
        var converter = new AnyScalarConverter();
        var literal = new BooleanValue { Value = true };
        
        // When
        var result = converter.ParseLiteral(literal);
        
        // Then
        Assert.Equal(literal, result);
    }

    [Fact]
    public void ParseLiteral_ShouldHandleEnumValue()
    {
        // Given
        var converter = new AnyScalarConverter();
        var literal = new EnumValue { Value = "ENUM_VALUE" };
        
        // When
        var result = converter.ParseLiteral(literal);
        
        // Then
        Assert.Equal(literal, result);
    }

    [Fact]
    public void ParseLiteral_ShouldHandleListValue()
    {
        // Given
        var converter = new AnyScalarConverter();
        var literal = new ListValue
        {
            Values = new[]
            {
                new StringValue { Value = "item1" },
                new IntValue { Value = 42 },
                new BooleanValue { Value = true }
            }
        };
        
        // When
        var result = converter.ParseLiteral(literal);
        
        // Then
        Assert.Equal(literal, result);
    }

    [Fact]
    public void ParseLiteral_ShouldHandleObjectValue()
    {
        // Given
        var converter = new AnyScalarConverter();
        var literal = new ObjectValue
        {
            Fields = new[]
            {
                new ObjectField
                {
                    Name = "field1",
                    Value = new StringValue { Value = "value1" }
                },
                new ObjectField
                {
                    Name = "field2",
                    Value = new IntValue { Value = 123 }
                }
            }
        };
        
        // When
        var result = converter.ParseLiteral(literal);
        
        // Then
        Assert.Equal(literal, result);
    }

    [Fact]
    public void ParseLiteral_ShouldHandleVariable()
    {
        // Given
        var converter = new AnyScalarConverter();
        var literal = new Variable { Name = "variableName" };
        
        // When
        var result = converter.ParseLiteral(literal);
        
        // Then
        Assert.Equal(literal, result);
    }

    [Fact]
    public void RoundTrip_SerializeAndParseValue_ShouldMaintainIntegrity()
    {
        // Given
        var converter = new AnyScalarConverter();
        var originalValue = new Dictionary<string, object>
        {
            { "name", "John" },
            { "age", 30 },
            { "active", true }
        };
        
        // When
        var serialized = converter.Serialize(originalValue);
        var parsed = converter.ParseValue(serialized);
        
        // Then
        Assert.Equal(originalValue, parsed);
    }

    [Fact]
    public void RoundTrip_SerializeAndParseLiteral_ShouldHandleStringValue()
    {
        // Given
        var converter = new AnyScalarConverter();
        var originalValue = "test value";
        
        // When
        var serialized = converter.Serialize(originalValue);
        var literal = new StringValue { Value = serialized.ToString() };
        var parsed = converter.ParseLiteral(literal);
        
        // Then
        Assert.Equal(literal, parsed);
    }

    [Fact]
    public void Converter_ShouldHandleEdgeCases()
    {
        // Given
        var converter = new AnyScalarConverter();
        var edgeCases = new object[]
        {
            string.Empty,
            0,
            0.0,
            false,
            new object[0],
            new Dictionary<string, object>()
        };
        
        // When & Then
        foreach (var edgeCase in edgeCases)
        {
            var serialized = converter.Serialize(edgeCase);
            var parsed = converter.ParseValue(serialized);
            Assert.Equal(edgeCase, parsed);
        }
    }

    [Fact]
    public void Converter_ShouldHandleNestedComplexObjects()
    {
        // Given
        var converter = new AnyScalarConverter();
        var nestedObject = new Dictionary<string, object>
        {
            { "user", new { Name = "John", Age = 30 } },
            { "tags", new[] { "tag1", "tag2" } },
            { "metadata", new Dictionary<string, object> { { "version", "1.0" } } }
        };
        
        // When
        var serialized = converter.Serialize(nestedObject);
        var parsed = converter.ParseValue(serialized);
        
        // Then
        Assert.Equal(nestedObject, parsed);
    }
}