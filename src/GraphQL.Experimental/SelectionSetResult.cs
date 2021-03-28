using System.Collections.Generic;

namespace Tanka.GraphQL.Experimental
{
    public record SelectionSetResult
    {
        public IReadOnlyDictionary<string, object> Data { get; init; }

        public IReadOnlyList<FieldError>? Errors { get; init; }
    }
}