using System.Collections.Generic;
using System.Text.Json.Serialization;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Server;

public class GraphQLHttpRequest
{
    [JsonPropertyName("operationName")]
    public string? OperationName { get; set; }

    [JsonPropertyName("query")]
    [JsonConverter(typeof(ExecutableDocumentConverter))]
    public ExecutableDocument Query { get; set; } = string.Empty;

    [JsonPropertyName("variables")]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    public IReadOnlyDictionary<string, object?>? Variables { get; set; }
}