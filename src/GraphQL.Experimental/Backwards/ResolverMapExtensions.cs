using System;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Experimental.Backwards
{
    public static class ResolverMapExtensions
    {
        public static void Add(this Resolvers map, string path, Resolver resolver)
        {
            var segments = path.Split('.');
            if (segments.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(path));

            var typeName = segments[0];
            var fieldName = segments[1];

            if (!map.TryGetValue(typeName, out var fields))
            {
                fields = new FieldResolversMap();
                map.Add(typeName, fields);
            }

            fields.Add(fieldName, resolver);
        }

        public static void Add(this Resolvers map, string path, Subscriber subscriber, Resolver resolver)
        {
            var segments = path.Split('.');
            if (segments.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(path));

            var typeName = segments[0];
            var fieldName = segments[1];

            if (!map.TryGetValue(typeName, out var fields))
            {
                fields = new FieldResolversMap();
                map.Add(typeName, fields);
            }

            fields.Add(fieldName, subscriber, resolver);
        }
    }
}