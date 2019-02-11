﻿using System.Collections.Generic;
using tanka.graphql.type;

namespace tanka.graphql.typeSystem
{
    public static class TypeDictionaryExtensions
    {
        public static bool TryGetValue<T>(this IDictionary<string, INamedType> types, string key, out T type)
            where T : INamedType
        {
            if (types.TryGetValue(key, out var value))
            {
                type = (T) value;
                return true;
            }

            type = default;
            return false;
        }
    }
}