using System;
using System.Collections.Generic;

namespace Tanka.GraphQL
{
    public static class ReadOnlyDictionaryExtensions
    {
        public static T GetValue<T>(this IReadOnlyDictionary<string, object> dictionary, string key, T defaultValue = default)
        {
            if (!dictionary.TryGetValue(key, out var value))
                return defaultValue;

            if (object.Equals(value, default(T)))
            {
                return default;
            }

            if (!(value is T typedValue))
            {
                throw new InvalidCastException(
                    $"Could not cast value type '{value.GetType().FullName}' to '{typeof(T).FullName}'");
            }

            return typedValue;
        }
    }
}