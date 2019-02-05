using System;
using System.Collections.Generic;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql
{
    public interface IResolverMap
    {
        Resolver GetResolver(string typeName, string fieldName);
    }

    public static class ResolverMapExtensions
    {
        public static Resolver GetResolver(this IResolverMap map, ComplexType type, KeyValuePair<string, IField> field)
        {
            return map.GetResolver(type.Name, field.Key);
        }
    }
}