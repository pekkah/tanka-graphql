using System.Collections.Generic;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql.schema
{
    public partial class ConnectionBuilder
    {
        public (
            Dictionary<string, Dictionary<string, IField>> _fields,
            Dictionary<string, Dictionary<string, InputObjectField>> _inputFields,
            Dictionary<string, Dictionary<string, ResolverBuilder>> _resolvers,
            Dictionary<string, Dictionary<string, SubscriberBuilder>> _subscribers) Build()
        {
            return (
                _fields,
                _inputFields,
                _resolvers,
                _subscribers);
        }
    }
}