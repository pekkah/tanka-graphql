using System.Collections.Generic;
using System.Text.Json;
using Tanka.GraphQL.Json;
using Xunit;

namespace Tanka.GraphQL.Tests;

public class NestedDictionaryConverterFacts
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new NestedDictionaryConverter()
        }
    };

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
        IReadOnlyDictionary<string, object> actual =
            JsonSerializer.Deserialize<IReadOnlyDictionary<string, object?>?>(json, Options);


        /* Then */
        object actualValue = actual.Select("representations", 1, "__typename");
        Assert.Equal("Product", actualValue);
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
        IReadOnlyDictionary<string, object> actual =
            JsonSerializer.Deserialize<IReadOnlyDictionary<string, object?>>(json, Options);


        /* Then */
        object actualValue = actual.Select("key", 0, "Product");
        Assert.Equal("test", actualValue);
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
        IReadOnlyDictionary<string, object> actual =
            JsonSerializer.Deserialize<IReadOnlyDictionary<string, object?>>(json, Options);


        /* Then */
        object actualValue = actual.Select("key", "nestedObject", "Product");
        Assert.Equal("test", actualValue);
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
        IReadOnlyDictionary<string, object> actual =
            JsonSerializer.Deserialize<IReadOnlyDictionary<string, object?>>(json, Options);


        /* Then */
        Assert.Equal("string", actual.NestedOrNull("key")!["nestedKey"]);
    }

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
        IReadOnlyDictionary<string, object> actual =
            JsonSerializer.Deserialize<IReadOnlyDictionary<string, object?>>(json, Options);


        /* Then */
        Assert.True(actual.ContainsKey("key"));
        Assert.Equal("string", actual["key"]);
    }
}