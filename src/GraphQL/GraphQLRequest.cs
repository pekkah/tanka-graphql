using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL;

/// <summary>
///     Execution options
/// </summary>
public record GraphQLRequest
{
    public required ExecutableDocument Document { get; init; }

    public object? InitialValue { get; set; }

    public string? OperationName { get; set; }

    public IReadOnlyDictionary<string, object?>? VariableValues { get; set; }
}