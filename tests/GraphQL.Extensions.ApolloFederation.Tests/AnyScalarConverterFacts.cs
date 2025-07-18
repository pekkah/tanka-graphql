using System;
using System.Collections.Generic;
using System.Text.Json;

using Tanka.GraphQL.Extensions.ApolloFederation;

using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

public class AnyScalarConverterFacts
{
    private readonly AnyScalarConverter _converter = new();

    [Fact]
    public void Serialize_StringValue_ReturnsString()
    {
        /* Given */
        var value = "test string";

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_IntValue_ReturnsInt()
    {
        /* Given */
        var value = 42;

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_FloatValue_ReturnsFloat()
    {
        /* Given */
        var value = 3.14f;

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_DoubleValue_ReturnsDouble()
    {
        /* Given */
        var value = 3.14159;

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_BooleanValue_ReturnsBoolean()
    {
        /* Given */
        var value = true;

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_NullValue_ReturnsNull()
    {
        /* Given */
        object? value = null;

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Null(result);
    }

    [Fact]
    public void Serialize_ArrayValue_ReturnsArray()
    {
        /* Given */
        var value = new[] { "a", "b", "c" };

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_ListValue_ReturnsList()
    {
        /* Given */
        var value = new List<string> { "a", "b", "c" };

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_DictionaryValue_ReturnsDictionary()
    {
        /* Given */
        var value = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 42 },
            { "key3", true }
        };

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_ComplexObject_ReturnsObject()
    {
        /* Given */
        var value = new { Name = "John", Age = 30, IsActive = true };

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_NestedObject_ReturnsNestedObject()
    {
        /* Given */
        var value = new
        {
            User = new { Name = "John", Age = 30 },
            Metadata = new Dictionary<string, object> { { "created", "2023-01-01" } }
        };

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_JsonElement_ReturnsJsonElement()
    {
        /* Given */
        var jsonString = """{ "name": "John", "age": 30 }""";
        var value = JsonSerializer.Deserialize<JsonElement>(jsonString);

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_DateTimeValue_ReturnsDateTime()
    {
        /* Given */
        var value = new DateTime(2023, 1, 1, 12, 0, 0);

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_DateTimeOffsetValue_ReturnsDateTimeOffset()
    {
        /* Given */
        var value = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero);

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_GuidValue_ReturnsGuid()
    {
        /* Given */
        var value = Guid.NewGuid();

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_EnumValue_ReturnsEnum()
    {
        /* Given */
        var value = StringComparison.OrdinalIgnoreCase;

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_DecimalValue_ReturnsDecimal()
    {
        /* Given */
        var value = 123.45m;

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_ByteArrayValue_ReturnsByteArray()
    {
        /* Given */
        var value = new byte[] { 1, 2, 3, 4, 5 };

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_UriValue_ReturnsUri()
    {
        /* Given */
        var value = new Uri("https://example.com");

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_TimeSpanValue_ReturnsTimeSpan()
    {
        /* Given */
        var value = TimeSpan.FromMinutes(30);

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_LongValue_ReturnsLong()
    {
        /* Given */
        var value = 9223372036854775807L;

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_ShortValue_ReturnsShort()
    {
        /* Given */
        short value = 32767;

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_ByteValue_ReturnsByte()
    {
        /* Given */
        byte value = 255;

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_CharValue_ReturnsChar()
    {
        /* Given */
        var value = 'A';

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_ComplexNestedStructure_ReturnsStructure()
    {
        /* Given */
        var value = new Dictionary<string, object>
        {
            { "users", new List<object>
                {
                    new { id = 1, name = "John", active = true },
                    new { id = 2, name = "Jane", active = false }
                }
            },
            { "metadata", new Dictionary<string, object>
                {
                    { "total", 2 },
                    { "created", new DateTime(2023, 1, 1) }
                }
            }
        };

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_EmptyArray_ReturnsEmptyArray()
    {
        /* Given */
        var value = new object[0];

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_EmptyDictionary_ReturnsEmptyDictionary()
    {
        /* Given */
        var value = new Dictionary<string, object>();

        /* When */
        var result = _converter.Serialize(value);

        /* Then */
        Assert.Equal(value, result);
    }

    [Fact]
    public void Serialize_NullablePrimitives_ReturnsCorrectValues()
    {
        /* Given */
        int? nullableInt = 42;
        bool? nullableBool = true;
        double? nullableDouble = 3.14;
        int? nullInt = null;

        /* When */
        var intResult = _converter.Serialize(nullableInt);
        var boolResult = _converter.Serialize(nullableBool);
        var doubleResult = _converter.Serialize(nullableDouble);
        var nullResult = _converter.Serialize(nullInt);

        /* Then */
        Assert.Equal(42, intResult);
        Assert.Equal(true, boolResult);
        Assert.Equal(3.14, doubleResult);
        Assert.Null(nullResult);
    }

    [Fact]
    public void Serialize_SelfReferencingObject_HandlesCorrectly()
    {
        /* Given */
        var user = new User { Name = "John", Age = 30 };
        user.Friend = user; // Self-reference

        /* When */
        var result = _converter.Serialize(user);

        /* Then */
        Assert.NotNull(result);
        Assert.Equal(user, result);
    }

    [Fact]
    public void Serialize_CircularReference_HandlesCorrectly()
    {
        /* Given */
        var user1 = new User { Name = "John", Age = 30 };
        var user2 = new User { Name = "Jane", Age = 25 };
        user1.Friend = user2;
        user2.Friend = user1; // Circular reference

        /* When */
        var result = _converter.Serialize(user1);

        /* Then */
        Assert.NotNull(result);
        Assert.Equal(user1, result);
    }

    public class User
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public User? Friend { get; set; }
    }
}