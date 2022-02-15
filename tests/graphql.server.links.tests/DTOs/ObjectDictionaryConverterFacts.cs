using System.Collections.Generic;
using System.Text.Json;
using Tanka.GraphQL.Server.Links.DTOs;
using Xunit;

namespace Tanka.GraphQL.Server.Links.Tests.DTOs;

public class ObjectDictionaryConverterFacts
{
    private readonly JsonSerializerOptions _options;

    public ObjectDictionaryConverterFacts()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            IgnoreNullValues = false,
            Converters =
            {
                new ObjectDictionaryConverter()
            }
        };
    }

    [Fact]
    public void Deserialize_SimpleValues()
    {
        /* Given */
        var json = @"
                {
                    ""int"": 123,
                    ""double"": 123.456,
                    ""string"": ""string"",
                    ""bool"": true
                }
            ";

        /* When */
        var actual = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _options);

        /* Then */
        Assert.Equal(123, actual["int"]);
        Assert.Equal(123.456, actual["double"]);
        Assert.Equal("string", actual["string"]);
        Assert.Equal(true, actual["bool"]);
    }

    [Fact]
    public void Deserialize_Simple_Null()
    {
        /* Given */
        var json = @"
                {
                    ""string"": null
                }
            ";

        /* When */
        var actual = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _options);

        /* Then */
        Assert.Null(actual["string"]);
    }

    [Fact]
    public void Deserialize_Array()
    {
        /* Given */
        var json = @"
                {
                    ""values"": [1, 2, 3]
                }
            ";

        /* When */
        var actual = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _options);

        /* Then */
        Assert.NotNull(actual["values"]);
    }

    [Fact]
    public void Deserialize_Array_in_Array()
    {
        /* Given */
        var json = @"
                {
                    ""values"": [[1,2,3]]
                }
            ";

        /* When */
        var actual = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _options);

        /* Then */
        Assert.NotNull(actual["values"]);
        var values = actual["values"];
        Assert.IsAssignableFrom<IEnumerable<object>>(values);
    }

    [Fact]
    public void Deserialize_ComplexValue()
    {
        /* Given */
        var json = @"
                {
                    ""complex"": {
                        ""int"": 123,
                        ""double"": 123.456,
                        ""string"": ""string"",
                        ""bool"": true
                    }
                }
            ";

        /* When */
        var actual = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _options);

        /* Then */
        Assert.IsAssignableFrom<IDictionary<string, object>>(actual["complex"]);
        var complex = (IDictionary<string, object>)actual["complex"];
        Assert.Equal(123, complex["int"]);
        Assert.Equal(123.456, complex["double"]);
        Assert.Equal("string", complex["string"]);
        Assert.Equal(true, complex["bool"]);
    }

    [Fact]
    public void Deserialize_MixedValue()
    {
        /* Given */
        var json = @"
                {
                    ""int"": 123,
                    ""complex"": {
                        ""int"": 123,
                        ""double"": 123.456,
                        ""string"": ""string"",
                        ""bool"": true
                    },
                    ""bool"": true
                }
            ";

        /* When */
        var actual = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _options);

        /* Then */
        Assert.Equal(123, actual["int"]);
        Assert.Equal(true, actual["bool"]);

        Assert.IsAssignableFrom<IDictionary<string, object>>(actual["complex"]);
        var complex = (IDictionary<string, object>)actual["complex"];
        Assert.Equal(123, complex["int"]);
        Assert.Equal(123.456, complex["double"]);
        Assert.Equal("string", complex["string"]);
        Assert.Equal(true, complex["bool"]);
    }

    [Fact]
    public void Deserialize_Nested_SimpleValues()
    {
        /* Given */
        var json = @"
                {
                    ""value1"": ""string"",
                    ""dictionary"": {
                        ""int"": 123,
                        ""double"": 123.456,
                        ""string"": ""string"",
                        ""bool"": true
                    },
                    ""value2"": 123
                }
            ";

        /* When */
        var actual = JsonSerializer.Deserialize<Nested>(json, _options);

        /* Then */
        Assert.Equal("string", actual.Value1);
        Assert.Equal(123, actual.Value2);
    }

    [Fact]
    public void Serialize_SimpleValues()
    {
        /* Given */
        var source = new Nested
        {
            Value2 = 123,
            Value1 = null
        };

        /* When */
        var json = JsonSerializer.Serialize(source, _options);

        /* Then */
        Assert.Equal(
            @"{
  ""dictionary"": null,
  ""value1"": null,
  ""value2"": 123
}".Trim().ReplaceLineEndings(),
            json);
    }

    [Fact]
    public void Serialize_Nested_SimpleValues()
    {
        /* Given */
        var source = new Nested
        {
            Dictionary = new Dictionary<string, object>
            {
                ["int"] = 123,
                ["string"] = "string"
            },
            Value2 = 123,
            Value1 = "string"
        };

        /* When */
        var json = JsonSerializer.Serialize(source, _options);

        /* Then */
        Assert.Equal(
            @"{
  ""dictionary"": {
    ""int"": 123,
    ""string"": ""string""
  },
  ""value1"": ""string"",
  ""value2"": 123
}".Trim().ReplaceLineEndings(),
            json);
    }

    [Fact]
    public void Serialize_Nested_Simple_Null()
    {
        /* Given */
        var source = new Nested
        {
            Dictionary = new Dictionary<string, object>
            {
                ["string"] = null
            },
            Value2 = 123,
            Value1 = "string"
        };

        /* When */
        var json = JsonSerializer.Serialize(source, _options);

        /* Then */
        Assert.Equal(
            @"{
  ""dictionary"": {
    ""string"": null
  },
  ""value1"": ""string"",
  ""value2"": 123
}".Trim().ReplaceLineEndings(),
            json);
    }

    [Fact]
    public void Serialize_Nested_ComplexValues()
    {
        /* Given */
        var source = new Nested
        {
            Dictionary = new Dictionary<string, object>
            {
                ["int"] = 123,
                ["string"] = "string",
                ["complex"] = new Dictionary<string, object>
                {
                    ["double"] = 1.123d
                }
            },
            Value2 = 123,
            Value1 = "string"
        };

        /* When */
        var json = JsonSerializer.Serialize(source, _options);

        /* Then */
        Assert.Equal(
            @"{
  ""dictionary"": {
    ""int"": 123,
    ""string"": ""string"",
    ""complex"": {
      ""double"": 1.123
    }
  },
  ""value1"": ""string"",
  ""value2"": 123
}".Trim().ReplaceLineEndings(),
            json);
    }

    private class Nested
    {
        public Dictionary<string, object> Dictionary { get; set; }
        public string Value1 { get; set; }

        public int Value2 { get; set; }
    }
}