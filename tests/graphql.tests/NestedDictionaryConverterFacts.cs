using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Tanka.GraphQL.Tests;

public class NestedDictionaryConverterFacts
{
    public NestedDictionaryConverterFacts()
    {
        Options = new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new NestedDictionaryConverter()
            }
        };
    }

    public JsonSerializerOptions Options { get; }

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
}