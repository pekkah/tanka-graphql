using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL;
using Tanka.GraphQL.Extensions.ApolloFederation;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace GraphQL.Dev.Reviews
{
    public static class SchemaFactory
    {
        public static async ValueTask<ISchema> Create()
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
";
            var builder = new SchemaBuilder()
                .AddFederationDirectives();

            builder.Sdl(typeDefs);

            await Task.Delay(0);

            builder.AddFederationSchemaExtensions(
                new Dictionary<string, ResolveReference>
                {
                    ["User"] = UserReference,
                    ["Product"] = ProductReference
                });

            builder.UseResolversAndSubscribers(
                new ObjectTypeMap
                {
                    ["User"] = new FieldResolversMap
                    {
                        {"id", Resolve.PropertyOf<User>(u => u.ID)},
                        {"username", UserUsername},
                        {"reviews", UserReviews}
                    },
                    ["Review"] = new FieldResolversMap
                    {
                        {"id", Resolve.PropertyOf<Review>(r => r.ID)},
                        {"body", Resolve.PropertyOf<Review>(r => r.Body)},
                        {"author", ReviewAuthor},
                        {"product", Resolve.PropertyOf<Review>(r => r.Product)}
                    },
                    ["Product"] = new FieldResolversMap
                    {
                        {"upc", Resolve.PropertyOf<Product>(p => p.Upc)},
                        {"reviews", ProductReviews}
                    }
                });

            return SchemaTools.MakeExecutableSchemaWithIntrospection(builder);
        }

        private static ValueTask<IResolverResult> UserUsername(IResolverContext context)
        {
            var user = (User) context.ObjectValue;

            return ResolveSync.As(user.Username);
        }

        private static async ValueTask<(object Reference, INamedType? NamedType)> ProductReference(
            IResolverContext context, IReadOnlyDictionary<string, object> representation, INamedType type)
        {
            await Task.Delay(0);

            var upc = representation["upc"].ToString();
            var product = new Product
            {
                Upc = upc
            };

            return (product, type);
        }

        private static ValueTask<IResolverResult> ProductReviews(IResolverContext context)
        {
            var product = (Product) context.ObjectValue;
            var reviews = Db.Reviews
                .Where(r => r.Value.Product.Upc == product.Upc)
                .Select(p => p.Value);

            return ResolveSync.As(reviews);
        }

        private static ValueTask<IResolverResult> ReviewAuthor(IResolverContext context)
        {
            var review = (Review) context.ObjectValue;

            return ResolveSync.As(new User
            {
                ID = review.AuthorID,
                Username = Db.Usernames[review.AuthorID]
            });
        }

        private static ValueTask<IResolverResult> UserReviews(IResolverContext context)
        {
            var user = (User) context.ObjectValue;
            var reviews = Db.Reviews
                .Where(r => r.Value.AuthorID == user.ID)
                .Select(r => r.Value);

            return ResolveSync.As(reviews);
        }

        private static async ValueTask<(object Reference, INamedType? NamedType)> UserReference(
            IResolverContext context, IReadOnlyDictionary<string, object> representation, INamedType type)
        {
            await Task.Delay(0);

            if (!representation.TryGetValue("id", out var idObj))
            {
                throw new ArgumentOutOfRangeException("id", "Representation is missing the required 'id' value");
            }

            var userId = idObj.ToString();

            if (!Db.Usernames.TryGetValue(userId, out var username))
            {
                throw new ArgumentOutOfRangeException("id", $"User '{userId} not found");
            }

            var user = new User
            {
                ID = userId,
                Username = username
            };

            return (user, type);
        }
    }

    public static class Db
    {
        public static Dictionary<string, Review> Reviews { get; } = new Dictionary<string, Review>
        {
            ["1"] = new Review
            {
                ID = "1",
                AuthorID = "1",
                Product = new Product {Upc = "1"},
                Body = "Love it!"
            },
            ["2"] = new Review
            {
                ID = "2",
                AuthorID = "1",
                Product = new Product {Upc = "2"},
                Body = "Too expensive!"
            },
            ["3"] = new Review
            {
                ID = "3",
                AuthorID = "2",
                Product = new Product {Upc = "3"},
                Body = "Could be better"
            },
            ["4"] = new Review
            {
                ID = "4",
                AuthorID = "2",
                Product = new Product {Upc = "1"},
                Body = "Prefer something else"
            }
        };

        public static Dictionary<string, string> Usernames { get; } = new Dictionary<string, string>
        {
            ["1"] = "@ada",
            ["2"] = "@complete"
        };
    }

    public class Review
    {
        public string ID { get; set; }

        public string Body { get; set; }

        public string AuthorID { get; set; }

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
}