using System.Collections.Generic;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql.schema
{
    public partial class ConnectionBuilder
    {
        public (
            IReadOnlyDictionary<string, Dictionary<string, IField>> _fields,
            IReadOnlyDictionary<string, Dictionary<string, InputObjectField>> _inputFields,
            IReadOnlyDictionary<string, Dictionary<string, ResolverBuilder>> _resolvers,
            IReadOnlyDictionary<string, Dictionary<string, SubscriberBuilder>> _subscribers) Build()
        {
            return (
                _fields,
                _inputFields,
                _resolvers,
                _subscribers);
        }
    }
}