using System.Collections.Generic;

namespace Tanka.GraphQL.Experimental
{
    public record SelectionSetResult
    {
        public IReadOnlyDictionary<string, object?>? Data { get; init; }

        public IReadOnlyList<FieldException>? Errors { get; init; }
    }
}