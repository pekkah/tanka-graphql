using System.Collections.Generic;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL
{
    public class ObjectTypeMap : Dictionary<string, FieldResolversMap>, IResolverMap, ISubscriberMap
    {
        public Resolver GetResolver(string typeName, string fieldName)
        {
            if (!TryGetValue(typeName, out var objectNode))
                return null;

            var resolver = objectNode.GetResolver(fieldName);
            return resolver;
        }

        public Subscriber GetSubscriber(string typeName, string fieldName)
        {
            if (!TryGetValue(typeName, out var objectNode))
                return null;

            var resolver = objectNode.GetSubscriber(fieldName);
            return resolver;
        }
    }
}