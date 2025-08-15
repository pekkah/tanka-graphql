using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

public static class SchemaFactory
{
    public static async Task<ISchema> Create()
    {
        var typeDefs = @"
schema @link(url: ""https://specs.apollo.dev/federation/v2.3"", import: [""@key"", ""@extends"", ""@external"", ""@provides"", ""_Entity"", ""_Any"", ""_Service""]) {
  query: Query
}

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

        var referenceResolvers = new DictionaryReferenceResolversMap
        {
            ["User"] = UserReference,
            ["Product"] = ProductReference
        };

        var builder = new ExecutableSchemaBuilder();
        builder.Add(typeDefs);
        builder.Add(new ResolversMap
        {
            ["User"] = new()
            {
                { "id", ctx => ctx.ResolveAsPropertyOf<User>(u => u.ID) },
                { "username", UserUsername },
                { "reviews", UserReviews }
            },
            ["Review"] = new()
            {
                { "id", ctx => ctx.ResolveAsPropertyOf<Review>(r => r.ID) },
                { "body", ctx => ctx.ResolveAsPropertyOf<Review>(r => r.Body) },
                { "author", ReviewAuthor },
                { "product", ctx => ctx.ResolveAsPropertyOf<Review>(r => r.Product) }
            },
            ["Product"] = new()
            {
                { "upc", ctx => ctx.ResolveAsPropertyOf<Product>(p => p.Upc) },
                { "reviews", ProductReviews }
            }
        });

        var schema = await builder.Build(options =>
        {
            options.UseFederation(new SubgraphOptions(referenceResolvers));
        });

        return schema;
    }

    private static ValueTask UserUsername(ResolverContext context)
    {
        var user = (User)context.ObjectValue;
        context.ResolvedValue = user.Username;
        return default;
    }

    private static ValueTask<ResolveReferenceResult> ProductReference(
        ResolverContext context,
        TypeDefinition typeDefinition,
        IReadOnlyDictionary<string, object> representation)
    {
        var upc = representation["upc"].ToString();
        var product = new Product
        {
            Upc = upc
        };

        return new(new ResolveReferenceResult(typeDefinition, product));
    }

    private static ValueTask ProductReviews(ResolverContext context)
    {
        var product = (Product)context.ObjectValue;
        var reviews = Db.Reviews
            .Where(r => r.Value.Product.Upc == product.Upc)
            .Select(p => p.Value);

        context.ResolvedValue = reviews;
        return default;
    }

    private static ValueTask ReviewAuthor(ResolverContext context)
    {
        var review = (Review)context.ObjectValue;

        return context.ResolveAs(new User
        {
            ID = review.AuthorID,
            Username = Db.Usernames[review.AuthorID]
        });
    }

    private static ValueTask UserReviews(ResolverContext context)
    {
        var user = (User)context.ObjectValue;
        var reviews = Db.Reviews
            .Where(r => r.Value.AuthorID == user.ID)
            .Select(r => r.Value);

        return context.ResolveAs(reviews);
    }

    private static ValueTask<ResolveReferenceResult> UserReference(
        ResolverContext context,
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

        return new(new ResolveReferenceResult(typeDefinition, user));
    }
}

public static class Db
{
    public static Dictionary<string, Review> Reviews { get; } = new()
    {
        ["1"] = new()
        {
            ID = "1",
            AuthorID = "1",
            Product = new() { Upc = "1" },
            Body = "Love it!"
        },
        ["2"] = new()
        {
            ID = "2",
            AuthorID = "1",
            Product = new() { Upc = "2" },
            Body = "Too expensive!"
        },
        ["3"] = new()
        {
            ID = "3",
            AuthorID = "2",
            Product = new() { Upc = "3" },
            Body = "Could be better"
        },
        ["4"] = new()
        {
            ID = "4",
            AuthorID = "2",
            Product = new() { Upc = "1" },
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