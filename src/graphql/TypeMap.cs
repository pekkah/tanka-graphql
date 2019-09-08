using System.Collections.Generic;
using tanka.graphql.resolvers;

namespace tanka.graphql
{
    public class TypeMap : Dictionary<string, FieldResolversMap>, IResolverMap, ISubscriberMap
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