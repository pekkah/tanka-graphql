using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public record RequestOptions
    {
        public ExecutableDocument? Document { get; init; }

        public ExecutableSchema? Schema { get; init; }

        public IReadOnlyDictionary<string, object>? VariableValues { get; init; }
    }
}