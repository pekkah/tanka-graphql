using System;
using System.Linq;
using tanka.graphql.type;

namespace tanka.graphql.graph
{
    public static class SchemaExtensions
    {
        public static T GetNamedType<T>(this ISchema schema, string name) where T : INamedType
        {
            return (T) schema.GetNamedType(name);
        }

        public static ISchema WithRoots(this ISchema schema,
            Func<(ObjectType query, ObjectType mutation, ObjectType subscription),
                Func<string, INamedType>,
                (ObjectType query, ObjectType mutation, ObjectType subscription)> with)
        {
            var (query, mutation, subscription) =
                with((schema.Query, schema.Mutation, schema.Subscription), schema.GetNamedType);

            return Schema.Initialize(
                query,
                mutation,
                subscription,
                directiveTypes: schema.QueryDirectives(),
                transforms: Enumerable.Empty<SchemaTransform>().ToArray());
        }
    }
}