using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests.Execution;

public class ValueCompletionFacts
{
    private readonly ISchema _schema;
    private readonly ResolversMap _resolvers;
    private readonly IServiceProvider _serviceProvider;

    public ValueCompletionFacts()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        _serviceProvider = services.BuildServiceProvider();

        var sdl = @"
            interface Node {
                id: ID!
            }

            type User implements Node {
                id: ID!
                name: String!
                email: String!
            }

            type Product implements Node {
                id: ID!
                name: String!
                price: Float!
            }

            union SearchResult = User | Product

            type Query {
                node(id: ID!): Node
                search(query: String!): SearchResult
                nodes: [Node!]!
                searchResults: [SearchResult!]!
                nullableNode: Node
                circularReference: User
            }
        ";

        _resolvers = new ResolversMap
        {
            {
                "Node", new FieldResolversMap
                {
                    { "id", context => context.ResolveAsPropertyOf<INode>(n => n.Id) }
                }
            },
            {
                "User", new FieldResolversMap
                {
                    { "id", context => context.ResolveAsPropertyOf<User>(u => u.Id) },
                    { "name", context => context.ResolveAsPropertyOf<User>(u => u.Name) },
                    { "email", context => context.ResolveAsPropertyOf<User>(u => u.Email) }
                }
            },
            {
                "Product", new FieldResolversMap
                {
                    { "id", context => context.ResolveAsPropertyOf<Product>(p => p.Id) },
                    { "name", context => context.ResolveAsPropertyOf<Product>(p => p.Name) },
                    { "price", context => context.ResolveAsPropertyOf<Product>(p => p.Price) }
                }
            },
            {
                "Query", new FieldResolversMap
                {
                    { 
                        "node", context => 
                        {
                            var id = context.GetArgument<string>("id");
                            if (id == "user1")
                            {
                                var user = new User { Id = "user1", Name = "John", Email = "john@example.com" };
                                context.ResolvedValue = user;
                                context.ResolveAbstractType = (_, _) => context.Schema.GetRequiredNamedType<ObjectDefinition>("User");
                            }
                            else if (id == "product1")
                            {
                                var product = new Product { Id = "product1", Name = "Widget", Price = 19.99f };
                                context.ResolvedValue = product;
                                context.ResolveAbstractType = (_, _) => context.Schema.GetRequiredNamedType<ObjectDefinition>("Product");
                            }
                            else
                            {
                                context.ResolvedValue = null;
                            }
                        }
                    },
                    { 
                        "search", context => 
                        {
                            var query = context.GetArgument<string>("query");
                            if (query == "john")
                            {
                                var user = new User { Id = "user1", Name = "John", Email = "john@example.com" };
                                context.ResolvedValue = user;
                                context.ResolveAbstractType = (_, _) => context.Schema.GetRequiredNamedType<ObjectDefinition>("User");
                            }
                            else if (query == "widget")
                            {
                                var product = new Product { Id = "product1", Name = "Widget", Price = 19.99f };
                                context.ResolvedValue = product;
                                context.ResolveAbstractType = (_, _) => context.Schema.GetRequiredNamedType<ObjectDefinition>("Product");
                            }
                            else
                            {
                                context.ResolvedValue = null;
                            }
                        }
                    },
                    { 
                        "nodes", context => 
                        {
                            var nodes = new List<INode>
                            {
                                new User { Id = "user1", Name = "John", Email = "john@example.com" },
                                new Product { Id = "product1", Name = "Widget", Price = 19.99f }
                            };
                            context.ResolvedValue = nodes;
                            context.ResolveAbstractType = (_, obj) => obj switch
                            {
                                User => context.Schema.GetRequiredNamedType<ObjectDefinition>("User"),
                                Product => context.Schema.GetRequiredNamedType<ObjectDefinition>("Product"),
                                _ => throw new InvalidOperationException("Unknown type")
                            };
                        }
                    },
                    { 
                        "searchResults", context => 
                        {
                            var results = new List<object>
                            {
                                new User { Id = "user1", Name = "John", Email = "john@example.com" },
                                new Product { Id = "product1", Name = "Widget", Price = 19.99f }
                            };
                            context.ResolvedValue = results;
                            context.ResolveAbstractType = (_, obj) => obj switch
                            {
                                User => context.Schema.GetRequiredNamedType<ObjectDefinition>("User"),
                                Product => context.Schema.GetRequiredNamedType<ObjectDefinition>("Product"),
                                _ => throw new InvalidOperationException("Unknown type")
                            };
                        }
                    },
                    { 
                        "nullableNode", context => 
                        {
                            context.ResolvedValue = null;
                        }
                    },
                    { 
                        "circularReference", context => 
                        {
                            var user = new User { Id = "user1", Name = "John", Email = "john@example.com" };
                            // This could potentially create a circular reference in complex scenarios
                            context.ResolvedValue = user;
                            context.ResolveAbstractType = (_, _) => context.Schema.GetRequiredNamedType<ObjectDefinition>("User");
                        }
                    }
                }
            }
        };

        _schema = new SchemaBuilder()
            .Add(sdl)
            .Build(_resolvers, _resolvers).Result;
    }

    [Fact]
    public async Task InterfaceResolution_WithUser_ReturnsCorrectType()
    {
        /* Given */
        var query = @"
            query {
                node(id: ""user1"") {
                    id
                    __typename
                    ... on User {
                        name
                        email
                    }
                }
            }";

        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(@"
        {
            ""data"": {
                ""node"": {
                    ""id"": ""user1"",
                    ""__typename"": ""User"",
                    ""name"": ""John"",
                    ""email"": ""john@example.com""
                }
            }
        }");
    }

    [Fact]
    public async Task InterfaceResolution_WithProduct_ReturnsCorrectType()
    {
        /* Given */
        var query = @"
            query {
                node(id: ""product1"") {
                    id
                    __typename
                    ... on Product {
                        name
                        price
                    }
                }
            }";

        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(@"
        {
            ""data"": {
                ""node"": {
                    ""id"": ""product1"",
                    ""__typename"": ""Product"",
                    ""name"": ""Widget"",
                    ""price"": 19.99
                }
            }
        }");
    }

    [Fact]
    public async Task InterfaceResolution_WithNonExistentId_ReturnsNull()
    {
        /* Given */
        var query = @"
            query {
                node(id: ""nonexistent"") {
                    id
                    __typename
                }
            }";

        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(@"
        {
            ""data"": {
                ""node"": null
            }
        }");
    }

    [Fact]
    public async Task UnionResolution_WithUser_ReturnsCorrectType()
    {
        /* Given */
        var query = @"
            query {
                search(query: ""john"") {
                    __typename
                    ... on User {
                        id
                        name
                        email
                    }
                }
            }";

        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(@"
        {
            ""data"": {
                ""search"": {
                    ""__typename"": ""User"",
                    ""id"": ""user1"",
                    ""name"": ""John"",
                    ""email"": ""john@example.com""
                }
            }
        }");
    }

    [Fact]
    public async Task UnionResolution_WithProduct_ReturnsCorrectType()
    {
        /* Given */
        var query = @"
            query {
                search(query: ""widget"") {
                    __typename
                    ... on Product {
                        id
                        name
                        price
                    }
                }
            }";

        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(@"
        {
            ""data"": {
                ""search"": {
                    ""__typename"": ""Product"",
                    ""id"": ""product1"",
                    ""name"": ""Widget"",
                    ""price"": 19.99
                }
            }
        }");
    }

    [Fact]
    public async Task InterfaceList_ReturnsCorrectTypes()
    {
        /* Given */
        var query = @"
            query {
                nodes {
                    id
                    __typename
                    ... on User {
                        name
                        email
                    }
                    ... on Product {
                        name
                        price
                    }
                }
            }";

        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(@"
        {
            ""data"": {
                ""nodes"": [
                    {
                        ""id"": ""user1"",
                        ""__typename"": ""User"",
                        ""name"": ""John"",
                        ""email"": ""john@example.com""
                    },
                    {
                        ""id"": ""product1"",
                        ""__typename"": ""Product"",
                        ""name"": ""Widget"",
                        ""price"": 19.99
                    }
                ]
            }
        }");
    }

    [Fact]
    public async Task UnionList_ReturnsCorrectTypes()
    {
        /* Given */
        var query = @"
            query {
                searchResults {
                    __typename
                    ... on User {
                        id
                        name
                        email
                    }
                    ... on Product {
                        id
                        name
                        price
                    }
                }
            }";

        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(@"
        {
            ""data"": {
                ""searchResults"": [
                    {
                        ""__typename"": ""User"",
                        ""id"": ""user1"",
                        ""name"": ""John"",
                        ""email"": ""john@example.com""
                    },
                    {
                        ""__typename"": ""Product"",
                        ""id"": ""product1"",
                        ""name"": ""Widget"",
                        ""price"": 19.99
                    }
                ]
            }
        }");
    }

    [Fact]
    public async Task NullableInterface_ReturnsNull()
    {
        /* Given */
        var query = @"
            query {
                nullableNode {
                    id
                    __typename
                }
            }";

        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(@"
        {
            ""data"": {
                ""nullableNode"": null
            }
        }");
    }

    [Fact]
    public async Task InterfaceResolution_WithInvalidType_ThrowsError()
    {
        /* Given */
        var invalidResolvers = new ResolversMap
        {
            {
                "Query", new FieldResolversMap
                {
                    { 
                        "node", context => 
                        {
                            // Return an object that doesn't implement the interface
                            context.ResolvedValue = new { Id = "invalid", Name = "Invalid" };
                            context.ResolveAbstractType = (_, _) => throw new InvalidOperationException("Cannot resolve abstract type");
                        }
                    }
                }
            }
        };

        var invalidSchema = new SchemaBuilder()
            .Add(@"
                interface Node {
                    id: ID!
                }

                type User implements Node {
                    id: ID!
                    name: String!
                }

                type Query {
                    node(id: ID!): Node
                }
            ")
            .Build(invalidResolvers, invalidResolvers).Result;

        var query = @"
            query {
                node(id: ""user1"") {
                    id
                    __typename
                }
            }";

        /* When */
        var result = await Executor.Execute(invalidSchema, query);

        /* Then */
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task CircularReference_HandledCorrectly()
    {
        /* Given */
        var query = @"
            query {
                circularReference {
                    id
                    name
                    email
                }
            }";

        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(@"
        {
            ""data"": {
                ""circularReference"": {
                    ""id"": ""user1"",
                    ""name"": ""John"",
                    ""email"": ""john@example.com""
                }
            }
        }");
    }

    public interface INode
    {
        string Id { get; }
    }

    public class User : INode
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class Product : INode
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public float Price { get; set; }
    }
}