using System;
using System.Collections.Generic;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql
{
    public class ResolverMap : Dictionary<string, FieldResolverMap>, IResolverMap, ISubscriberMap
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