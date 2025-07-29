using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Tanka.GraphQL.Json;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Request;

/// <summary>
///     Execution options
/// </summary>
public record GraphQLRequest
{
    public GraphQLRequest()
    {
    }

    [SetsRequiredMembers]
    public GraphQLRequest(ExecutableDocument query)
    {
        Query = query;
    }

    [JsonPropertyName("query")]
    [JsonConverter(typeof(ExecutableDocumentConverter))]
    public required ExecutableDocument Query { get; init; }

    [JsonPropertyName("initialValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? InitialValue { get; set; }

    [JsonPropertyName("operationName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OperationName { get; set; }

    [JsonPropertyName("variables")]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, object?>? Variables { get; set; }

    [JsonPropertyName("extensions")]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    public IReadOnlyDictionary<string, object?>? Extensions { get; set; }
}