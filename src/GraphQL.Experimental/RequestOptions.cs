using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public record RequestOptions
    {
        public string? OperationName { get; init; }

        public ExecutableDocument Document { get; init; } = null!;

        public ExecutableSchema Schema { get; init; } = null!;

        public IReadOnlyDictionary<string, object>? VariableValues { get; init; }
    }
}