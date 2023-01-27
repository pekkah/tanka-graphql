using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL;

/// <summary>
///     Execution options
/// </summary>
public record GraphQLRequest
{
    public GraphQLRequest()
    {
    }

    [SetsRequiredMembers]
    public GraphQLRequest(ExecutableDocument document)
    {
        Document = document;
    }

    [JsonPropertyName("document")]
    [JsonConverter(typeof(ExecutableDocumentConverter))]
    public required ExecutableDocument Document { get; init; }

    [JsonPropertyName("initialValue")]
    public object? InitialValue { get; set; }

    [JsonPropertyName("operationName")]
    public string? OperationName { get; set; }

    [JsonPropertyName("variables")]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    public IReadOnlyDictionary<string, object?>? Variables { get; set; }
}