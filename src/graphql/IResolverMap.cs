using System;
using System.Collections.Generic;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL
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