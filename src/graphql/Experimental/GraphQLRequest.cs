using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental;

/// <summary>
///     Execution options
/// </summary>
public record GraphQLRequest(
    ExecutableDocument Document,
    object? InitialValue = default,
    string? OperationName = default,
    IReadOnlyDictionary<string, object?>? VariableValues = default);