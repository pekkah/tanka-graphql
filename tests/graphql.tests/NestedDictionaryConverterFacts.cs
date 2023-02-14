using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Xunit;

namespace Tanka.GraphQL.Tests;

public class NestedDictionaryConverterFacts
{
    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new NestedDictionaryConverter()
        }
    };

    [Fact]
    public void Deserialize_simple_string_value()
    {
        /* Given */
        var json = """
                {
                    "key":"string"
                }
                """;

        /* When */
        var actual = JsonSerializer.Deserialize<IReadOnlyDictionary<string, object?>>(json, Options);


        /* Then */
        Assert.True(actual.ContainsKey("key"));
        Assert.Equal("string", actual["key"]);
    }

    [Fact]
    public void Deserialize_nested_string_value()
    {
        /* Given */
        var json = """
                {
                    "key": {
                        "nestedKey": "string"
                    }
                }
                """;

        /* When */
        var actual = JsonSerializer.Deserialize<IReadOnlyDictionary<string, object?>>(json, Options);


        /* Then */
        Assert.Equal("string", actual.NestedOrNull("key")!["nestedKey"]);
    }

    [Fact]
    public void Deserialize_nested_object_with_object_with_string_value()
    {
        /* Given */
        var json = """
                {
                    "key": {
                        "nestedObject": {
                            "Product": "test"
                        }
                    }
                }
                """;

        /* When */
        var actual = JsonSerializer.Deserialize<IReadOnlyDictionary<string, object?>>(json, Options);


        /* Then */
        var actualValue = actual.Select("key", "nestedObject", "Product");
        Assert.Equal("test", actualValue);
    }

    [Fact]
    public void Deserialize_nested_array_with_object_value_with_string_property()
    {
        /* Given */
        var json = """
                {
                    "key": [{
                        "Product": "test"
                        }]                    
                }
                """;

        /* When */
        var actual = JsonSerializer.Deserialize<IReadOnlyDictionary<string, object?>>(json, Options);


        /* Then */
        var actualValue = actual.Select("key", 0, "Product");
        Assert.Equal("test", actualValue);
    }

    [Fact]
    public void Deserialize_federation_request()
    {
        /* Given */
        var json = """
                {
                "representations":[{"__typename":"Product","upc":"1"},{"__typename":"Product","upc":"2"},{ "__typename":"Product","upc":"3"}]
                }
                """;

        /* When */
        var actual = JsonSerializer.Deserialize<IReadOnlyDictionary<string, object?>?>(json, Options);


        /* Then */
        var actualValue = actual.Select( "representations", 1, "__typename");
        Assert.Equal("Product", actualValue);
    }
}