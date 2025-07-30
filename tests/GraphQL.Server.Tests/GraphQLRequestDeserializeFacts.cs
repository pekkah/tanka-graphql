using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class GraphQLRequestDeserializeFacts
{

    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    [StringSyntax("json")]
    public string Json = """
        {"query":"query($representations:[_Any!]!){_entities(representations:$representations){...on Product{reviews{author{__typename id}body}}}}","variables":{"representations":[{"__typename":"Product","upc":"1"},{"__typename":"Product","upc":"2"},{"__typename":"Product","upc":"3"}]}}
        """;

    [Fact]
    public void Deserialize()
    {
        /* Given */
        var json = Json;

        /* When */
        var actual = JsonSerializer.Deserialize<GraphQLHttpRequest>(json, Options);

        /* Then */
        Assert.NotNull(actual.Query);
        Assert.NotNull(actual.Variables);
        Assert.NotEmpty(actual.Variables);

    }
}