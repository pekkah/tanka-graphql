using System;
using System.Collections.Generic;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql
{
    public class ResolverMap : Dictionary<string, FieldResolverMap>, IResolverMap, ISubscriberMap
    {
        public Resolver GetResolver(ComplexType type, KeyValuePair<string, IField> field)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (field.Value == null)
                throw new ArgumentNullException(nameof(field));

            var fieldName = field.Key;

            if (!TryGetValue(type.Name, out var objectNode)) 
                return null;

            var resolver = objectNode.GetResolver(fieldName);

            if (resolver == null)
            {
                return null;
            }

            return resolver;
        }

        public Subscriber GetSubscriber(ComplexType type, KeyValuePair<string, IField> field)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (field.Value == null)
                throw new ArgumentNullException(nameof(field));

            var fieldName = field.Key;

            if (!TryGetValue(type.Name, out var objectNode)) return null;

            var resolver = objectNode.GetSubscriber(fieldName);
            return resolver;
        }
    }
}