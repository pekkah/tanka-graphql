using System.Collections.Generic;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL
{
    public class ResolversMap : Dictionary<string, FieldResolversMap>, IResolverMap, ISubscriberMap
    {
        public Resolver? GetResolver(string typeName, string fieldName)
        {
            if (!TryGetValue(typeName, out var objectNode))
                return null;

            var resolver = objectNode.GetResolver(fieldName);
            return resolver;
        }

        public Subscriber? GetSubscriber(string typeName, string fieldName)
        {
            if (!TryGetValue(typeName, out var objectNode))
                return null;

            var resolver = objectNode.GetSubscriber(fieldName);
            return resolver;
        }

        public ResolversMap Clone()
        {
            var result = new ResolversMap();

            foreach (var objectMap in this) result.Add(objectMap.Key, objectMap.Value.Clone());

            return result;
        }

        public static ResolversMap operator +(ResolversMap a, ResolversMap b)
        {
            var result = a.Clone();

            // override with b
            foreach (var objectMap in b) result[objectMap.Key] = objectMap.Value;

            return result;
        }

        public static ResolversMap operator +(ResolversMap a, (string Name, FieldResolversMap Fields) ObjectDefinition)
        {
            var result = a.Clone();

            if (result.ContainsKey(ObjectDefinition.Name))
                result[ObjectDefinition.Name] += ObjectDefinition.Fields;
            else
                result[ObjectDefinition.Name] = ObjectDefinition.Fields;

            return result;
        }

        public static ResolversMap operator -(ResolversMap a, string name)
        {
            var result = a.Clone();

            if (result.ContainsKey(name))
                result.Remove(name);

            return result;
        }

        public static ResolversMap operator -(ResolversMap a, ResolversMap b)
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