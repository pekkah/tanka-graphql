using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public static class ExecutableDirectiveLocations
    {
        public const string QUERY = nameof(QUERY);
        public const string MUTATION = nameof(MUTATION);
        public const string SUBSCRIPTION = nameof(SUBSCRIPTION);
        public const string FIELD = nameof(FIELD);
        public const string FRAGMENT_DEFINITION = nameof(FRAGMENT_DEFINITION);
        public const string FRAGMENT_SPREAD = nameof(FRAGMENT_SPREAD);
        public const string INLINE_FRAGMENT = nameof(INLINE_FRAGMENT);
        public const string VARIABLE_DEFINITION = nameof(VARIABLE_DEFINITION);

        public static IReadOnlyCollection<string> All = new List<string>
        {
            QUERY, 
            MUTATION, 
            SUBSCRIPTION, 
            FIELD, 
            FRAGMENT_SPREAD, 
            FRAGMENT_SPREAD, 
            INLINE_FRAGMENT, 
            VARIABLE_DEFINITION
        };
    }
}