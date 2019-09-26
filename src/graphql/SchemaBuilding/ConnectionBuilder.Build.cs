using System.Collections.Generic;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.SchemaBuilding
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