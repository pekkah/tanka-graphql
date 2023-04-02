using GraphQL.Dev.Reviews;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Server;

namespace Tanka.GraphQL.Dev.Reviews;

public static class SchemaOptionsBuilderExtensions
{
    public static OptionsBuilder<SchemaOptions> AddReviews(this OptionsBuilder<SchemaOptions> optionsBuilder)
    {
        optionsBuilder.Configure<ReviewsResolvers>((options, resolvers) =>
        {
            var builder = options.Builder;
            builder.Add("""
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

            builder.Add(resolvers);
        });

        return optionsBuilder;
    }
}