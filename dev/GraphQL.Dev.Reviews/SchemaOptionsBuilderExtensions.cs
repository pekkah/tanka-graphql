using Tanka.GraphQL.Server;

namespace GraphQL.Dev.Reviews;

public static class SchemaOptionsBuilderExtensions
{
    public static SchemaOptionsBuilder AddReviews(this SchemaOptionsBuilder options)
    {
        options.Configure<ReviewsResolvers>((schema, resolvers) =>
        {
            schema.Add("""
                type Review @key(fields: "id") {
                    id: ID!
                    body: String
                    author: User @provides(fields: "username")
                    product: Product
                }
                
                type User @key(fields: "id") @extends {
                    id: ID! @external
                    username: String @external
                    reviews: [Review]
                }
                
                type Product @key(fields: "upc") @extends {
                    upc: String! @external
                    reviews: [Review]
                }
                """);

            schema.Add(resolvers);
        });

        return options;
    }
}