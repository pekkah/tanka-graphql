using System.Collections.Generic;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL
{
    public class Resolvers : Dictionary<string, FieldResolversMap>, IResolverMap, ISubscriberMap
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

        public Resolvers Clone()
        {
            var result = new Resolvers();

            foreach (var objectMap in this) result.Add(objectMap.Key, objectMap.Value.Clone());

            return result;
        }

        public static Resolvers operator +(Resolvers a, Resolvers b)
        {
            var result = a.Clone();

            // override with b
            foreach (var objectMap in b) result[objectMap.Key] = objectMap.Value;

            return result;
        }

        public static Resolvers operator +(Resolvers a, (string Name, FieldResolversMap Fields) objectType)
        {
            var result = a.Clone();

            if (result.ContainsKey(objectType.Name))
                result[objectType.Name] += objectType.Fields;
            else
                result[objectType.Name] = objectType.Fields;

            return result;
        }

        public static Resolvers operator -(Resolvers a, string name)
        {
            var result = a.Clone();

            if (result.ContainsKey(name))
                result.Remove(name);

            return result;
        }

        public static Resolvers operator -(Resolvers a, Resolvers b)
        {
            var result = a.Clone();

            // remove b by key
            foreach (var objectMap in b)
                if (result.ContainsKey(objectMap.Key))
                    result.Remove(objectMap.Key);

            return result;
        }
    }
}