using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

public static class SchemaFactory
{
    public static async Task<ISchema> Create()
    {
        var typeDefs = @"
type Review @key(fields: ""id"") {
    id: ID!
    body: String
    author: User @provides(fields: ""username"")
    product: Product
}

type User @key(fields: ""id"") @extends {
    id: ID! @external
    username: String @external
    reviews: [Review]
}

type Product @key(fields: ""upc"") @extends {
    upc: String! @external
    reviews: [Review]
}

type Query {
}
";

        var builder = new SchemaBuilder();
        builder.Add(typeDefs);

        var resolvers = new ResolversMap
        {
            ["User"] = new()
            {
                { "id", Resolve.PropertyOf<User>(u => u.ID) },
                { "username", UserUsername },
                { "reviews", UserReviews }
            },
            ["Review"] = new()
            {
                { "id", Resolve.PropertyOf<Review>(r => r.ID) },
                { "body", Resolve.PropertyOf<Review>(r => r.Body) },
                { "author", ReviewAuthor },
                { "product", Resolve.PropertyOf<Review>(r => r.Product) }
            },
            ["Product"] = new()
            {
                { "upc", Resolve.PropertyOf<Product>(p => p.Upc) },
                { "reviews", ProductReviews }
            }
        };

        var schema = await builder.BuildSubgraph(new FederatedSchemaBuildOptions()
            {
                SchemaBuildOptions = new SchemaBuildOptions(resolvers),
                ReferenceResolvers = new DictionaryReferenceResolversMap
                {
                    ["User"] = UserReference,
                    ["Product"] = ProductReference
                }
            });

        return schema;
    }

    private static ValueTask<IResolverResult> UserUsername(IResolverContext context)
    {
        var user = (User)context.ObjectValue;

        return ResolveSync.As(user.Username);
    }

    private static ValueTask<ResolveReferenceResult> ProductReference(
        IResolverContext context,
        TypeDefinition typeDefinition,
        IReadOnlyDictionary<string, object> representation)
    {
        var upc = representation["upc"].ToString();
        var product = new Product
        {
            Upc = upc
        };

        return new ValueTask<ResolveReferenceResult>(new ResolveReferenceResult(typeDefinition, product));
    }

    private static ValueTask<IResolverResult> ProductReviews(IResolverContext context)
    {
        var product = (Product)context.ObjectValue;
        var reviews = Db.Reviews
            .Where(r => r.Value.Product.Upc == product.Upc)
            .Select(p => p.Value);

        return ResolveSync.As(reviews);
    }

    private static ValueTask<IResolverResult> ReviewAuthor(IResolverContext context)
    {
        var review = (Review)context.ObjectValue;

        return ResolveSync.As(new User
        {
            ID = review.AuthorID,
            Username = Db.Usernames[review.AuthorID]
        });
    }

    private static ValueTask<IResolverResult> UserReviews(IResolverContext context)
    {
        var user = (User)context.ObjectValue;
        var reviews = Db.Reviews
            .Where(r => r.Value.AuthorID == user.ID)
            .Select(r => r.Value);

        return ResolveSync.As(reviews);
    }

    private static ValueTask<ResolveReferenceResult> UserReference(
        IResolverContext context,
        TypeDefinition typeDefinition,
        IReadOnlyDictionary<string, object> representation)
    {
        if (!representation.TryGetValue("id", out var idObj))
            throw new ArgumentOutOfRangeException("id", "Representation is missing the required 'id' value");

        var userId = idObj.ToString();

        if (!Db.Usernames.TryGetValue(userId, out var username))
            throw new ArgumentOutOfRangeException("id", $"User '{userId} not found");

        var user = new User
        {
            ID = userId,
            Username = username
        };

        return new ValueTask<ResolveReferenceResult>(new ResolveReferenceResult(typeDefinition, user));
    }
}

public static class Db
{
    public static Dictionary<string, Review> Reviews { get; } = new()
    {
        ["1"] = new Review
        {
            ID = "1",
            AuthorID = "1",
            Product = new Product { Upc = "1" },
            Body = "Love it!"
        },
        ["2"] = new Review
        {
            ID = "2",
            AuthorID = "1",
            Product = new Product { Upc = "2" },
            Body = "Too expensive!"
        },
        ["3"] = new Review
        {
            ID = "3",
            AuthorID = "2",
            Product = new Product { Upc = "3" },
            Body = "Could be better"
        },
        ["4"] = new Review
        {
            ID = "4",
            AuthorID = "2",
            Product = new Product { Upc = "1" },
            Body = "Prefer something else"
        }
    };

    public static Dictionary<string, string> Usernames { get; } = new()
    {
        ["1"] = "@ada",
        ["2"] = "@complete"
    };
}

public class Review
{
    public string AuthorID { get; set; }

    public string Body { get; set; }
    public string ID { get; set; }

    public Product Product { get; set; }
}

public class User
{
    public string ID { get; set; }

    public string Username { get; set; }
}

public class Product
{
    public string Upc { get; set; }
}