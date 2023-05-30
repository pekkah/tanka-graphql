using System.Text.Json.Serialization;

using Tanka.GraphQL.Json;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Server;

public record GraphQLHttpRequest
{
    [JsonPropertyName("operationName")]
    public string? OperationName { get; set; }

    [JsonPropertyName("query")]
    [JsonConverter(typeof(ExecutableDocumentConverter))]
    public ExecutableDocument Query { get; set; } = string.Empty;

    [JsonPropertyName("variables")]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    public IReadOnlyDictionary<string, object?>? Variables { get; set; }

    [JsonPropertyName("extensions")]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    public IReadOnlyDictionary<string, object?>? Extensions { get; set; }

    public override string ToString()
    {
        return PrettyJsonLog.PrettyJson(this);
    }

}