using System;
using tanka.graphql.type;

namespace tanka.graphql.graph
{
    public static class SchemaExtensions
    {
        public static ISchema WithQuery(this ISchema schema, ObjectType query)
        {
            if (schema.Query == query)
            {
                return schema;
            }

            return new Schema(query);
        }

        public static ISchema WithRoots(this ISchema schema, 
            Func<(ObjectType query, ObjectType mutation, ObjectType subscription),
                Func<string, INamedType>,
                (ObjectType query, ObjectType mutation, ObjectType subscription)> with)
        {
            var (query, mutation, subscription) =
                with((schema.Query, schema.Mutation, schema.Subscription), schema.GetNamedType);

            return new Schema(
                query,
                mutation,
                subscription,
                /* typesReferencedByNameOnly: ?? */
                directives: schema.QueryDirectives());
        }
    }
}