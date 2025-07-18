using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

public class FederationDirectiveFacts
{
    [Fact]
    public async Task External_directive_field_included_in_sdl()
    {
        // Given
        var schema = await CreateSchemaWithExternalFields();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = "query { _service { sdl } }"
            });

        // Then
        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
        var sdl = result.Data["_service"]["sdl"].ToString();
        Assert.Contains("@external", sdl);
        Assert.Contains("username: String @external", sdl);
    }

    [Fact]
    public async Task External_directive_field_not_resolvable_directly()
    {
        // Given
        var schema = await CreateSchemaWithExternalFields();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ...on User {
                                id
                                # This should work as username is provided by @provides
                                username
                            }
                        }
                    }
                    """,
                Variables = new Dictionary<string, object>
                {
                    ["representations"] = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["__typename"] = "User",
                            ["id"] = 1
                        }
                    }
                }
            });

        // Then
        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task Provides_directive_field_included_in_sdl()
    {
        // Given
        var schema = await CreateSchemaWithProvidesDirective();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = "query { _service { sdl } }"
            });

        // Then
        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
        var sdl = result.Data["_service"]["sdl"].ToString();
        Assert.Contains("@provides", sdl);
        Assert.Contains("author: User @provides(fields: \"username\")", sdl);
    }

    [Fact]
    public async Task Provides_directive_allows_querying_external_fields()
    {
        // Given
        var schema = await CreateSchemaWithProvidesDirective();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ...on Review {
                                id
                                body
                                author {
                                    username
                                }
                            }
                        }
                    }
                    """,
                Variables = new Dictionary<string, object>
                {
                    ["representations"] = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["__typename"] = "Review",
                            ["id"] = 1
                        }
                    }
                }
            });

        // Then
        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
        var entities = result.Data["_entities"] as object[];
        Assert.NotNull(entities);
        Assert.Single(entities);
    }

    [Fact]
    public async Task Requires_directive_field_included_in_sdl()
    {
        // Given
        var schema = await CreateSchemaWithRequiresDirective();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = "query { _service { sdl } }"
            });

        // Then
        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
        var sdl = result.Data["_service"]["sdl"].ToString();
        Assert.Contains("@requires", sdl);
        Assert.Contains("fullName: String @requires(fields: \"firstName lastName\")", sdl);
    }

    [Fact]
    public async Task Requires_directive_field_can_be_resolved_with_dependencies()
    {
        // Given
        var schema = await CreateSchemaWithRequiresDirective();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ...on User {
                                id
                                fullName
                            }
                        }
                    }
                    """,
                Variables = new Dictionary<string, object>
                {
                    ["representations"] = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["__typename"] = "User",
                            ["id"] = 1,
                            ["firstName"] = "John",
                            ["lastName"] = "Doe"
                        }
                    }
                }
            });

        // Then
        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
        var entities = result.Data["_entities"] as object[];
        Assert.NotNull(entities);
        Assert.Single(entities);
    }

    [Fact]
    public async Task Multiple_directives_on_same_field_work_correctly()
    {
        // Given
        var schema = await CreateSchemaWithMultipleDirectives();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = "query { _service { sdl } }"
            });

        // Then
        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
        var sdl = result.Data["_service"]["sdl"].ToString();
        Assert.Contains("@external", sdl);
        Assert.Contains("@requires", sdl);
    }

    [Fact]
    public async Task Extends_directive_on_type_included_in_sdl()
    {
        // Given
        var schema = await CreateSchemaWithExtendsDirective();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = "query { _service { sdl } }"
            });

        // Then
        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
        var sdl = result.Data["_service"]["sdl"].ToString();
        Assert.Contains("@extends", sdl);
        Assert.Contains("type User @key(fields: \"id\") @extends", sdl);
    }

    [Fact]
    public async Task Complex_federation_schema_with_all_directives_works()
    {
        // Given
        var schema = await CreateComplexFederationSchema();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = "query { _service { sdl } }"
            });

        // Then
        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
        var sdl = result.Data["_service"]["sdl"].ToString();
        
        // Verify all directives are present
        Assert.Contains("@key", sdl);
        Assert.Contains("@external", sdl);
        Assert.Contains("@provides", sdl);
        Assert.Contains("@requires", sdl);
        Assert.Contains("@extends", sdl);
    }

    private static async Task<ISchema> CreateSchemaWithExternalFields()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") @extends {
                    id: ID! @external
                    username: String @external
                    reviews: [Review]
                }

                type Review @key(fields: ""id"") {
                    id: ID!
                    body: String
                    author: User @provides(fields: ""username"")
                }

                type Query {
                    # Empty query type
                }
            ")
            .AddSubgraph(new(new DictionaryReferenceResolversMap
            {
                ["User"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var user = new { id, username = $"@user{id}" };
                    return new(new ResolveReferenceResult(type, user));
                },
                ["Review"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var review = new { id, body = $"Review {id}", author = new { id = "1", username = "@user1" } };
                    return new(new ResolveReferenceResult(type, review));
                }
            }))
            .Add(new ResolversMap
            {
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "username", context => context.ResolveAsPropertyOf<dynamic>(u => u.username) },
                    { "reviews", context => context.ResolveAs(new[] { new { id = "1", body = "Review 1" } }) }
                },
                ["Review"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(r => r.id) },
                    { "body", context => context.ResolveAsPropertyOf<dynamic>(r => r.body) },
                    { "author", context => context.ResolveAsPropertyOf<dynamic>(r => r.author) }
                }
            });

        return await builder.Build();
    }

    private static async Task<ISchema> CreateSchemaWithProvidesDirective()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") @extends {
                    id: ID! @external
                    username: String @external
                }

                type Review @key(fields: ""id"") {
                    id: ID!
                    body: String
                    author: User @provides(fields: ""username"")
                }

                type Query {
                    # Empty query type
                }
            ")
            .AddSubgraph(new(new DictionaryReferenceResolversMap
            {
                ["Review"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var review = new { id, body = $"Review {id}", author = new { id = "1", username = "@author1" } };
                    return new(new ResolveReferenceResult(type, review));
                }
            }))
            .Add(new ResolversMap
            {
                ["Review"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(r => r.id) },
                    { "body", context => context.ResolveAsPropertyOf<dynamic>(r => r.body) },
                    { "author", context => context.ResolveAsPropertyOf<dynamic>(r => r.author) }
                },
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "username", context => context.ResolveAsPropertyOf<dynamic>(u => u.username) }
                }
            });

        return await builder.Build();
    }

    private static async Task<ISchema> CreateSchemaWithRequiresDirective()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") @extends {
                    id: ID! @external
                    firstName: String @external
                    lastName: String @external
                    fullName: String @requires(fields: ""firstName lastName"")
                }

                type Query {
                    # Empty query type
                }
            ")
            .AddSubgraph(new(new DictionaryReferenceResolversMap
            {
                ["User"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var firstName = representation.ContainsKey("firstName") ? representation["firstName"].ToString() : "Unknown";
                    var lastName = representation.ContainsKey("lastName") ? representation["lastName"].ToString() : "Unknown";
                    var user = new { id, firstName, lastName };
                    return new(new ResolveReferenceResult(type, user));
                }
            }))
            .Add(new ResolversMap
            {
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "firstName", context => context.ResolveAsPropertyOf<dynamic>(u => u.firstName) },
                    { "lastName", context => context.ResolveAsPropertyOf<dynamic>(u => u.lastName) },
                    { "fullName", context => 
                        {
                            var user = (dynamic)context.ObjectValue;
                            return context.ResolveAs($"{user.firstName} {user.lastName}");
                        }
                    }
                }
            });

        return await builder.Build();
    }

    private static async Task<ISchema> CreateSchemaWithMultipleDirectives()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") @extends {
                    id: ID! @external
                    email: String @external
                    displayName: String @external @requires(fields: ""email"")
                }

                type Query {
                    # Empty query type
                }
            ")
            .AddSubgraph(new(new DictionaryReferenceResolversMap
            {
                ["User"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var email = representation.ContainsKey("email") ? representation["email"].ToString() : "unknown@example.com";
                    var user = new { id, email };
                    return new(new ResolveReferenceResult(type, user));
                }
            }))
            .Add(new ResolversMap
            {
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "email", context => context.ResolveAsPropertyOf<dynamic>(u => u.email) },
                    { "displayName", context => 
                        {
                            var user = (dynamic)context.ObjectValue;
                            return context.ResolveAs($"Display name for {user.email}");
                        }
                    }
                }
            });

        return await builder.Build();
    }

    private static async Task<ISchema> CreateSchemaWithExtendsDirective()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") @extends {
                    id: ID! @external
                    additionalField: String
                }

                type Query {
                    # Empty query type
                }
            ")
            .AddSubgraph(new(new DictionaryReferenceResolversMap
            {
                ["User"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var user = new { id, additionalField = $"Additional data for {id}" };
                    return new(new ResolveReferenceResult(type, user));
                }
            }))
            .Add(new ResolversMap
            {
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "additionalField", context => context.ResolveAsPropertyOf<dynamic>(u => u.additionalField) }
                }
            });

        return await builder.Build();
    }

    private static async Task<ISchema> CreateComplexFederationSchema()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") @extends {
                    id: ID! @external
                    username: String @external
                    email: String @external
                    fullName: String @requires(fields: ""username email"")
                }

                type Product @key(fields: ""upc"") @extends {
                    upc: String! @external
                    name: String @external
                    reviews: [Review]
                }

                type Review @key(fields: ""id"") {
                    id: ID!
                    body: String
                    rating: Int
                    author: User @provides(fields: ""username"")
                    product: Product @provides(fields: ""name"")
                }

                type Query {
                    # Empty query type
                }
            ")
            .AddSubgraph(new(new DictionaryReferenceResolversMap
            {
                ["User"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var username = representation.ContainsKey("username") ? representation["username"].ToString() : $"@user{id}";
                    var email = representation.ContainsKey("email") ? representation["email"].ToString() : $"user{id}@example.com";
                    var user = new { id, username, email };
                    return new(new ResolveReferenceResult(type, user));
                },
                ["Product"] = (context, type, representation) =>
                {
                    var upc = representation["upc"].ToString();
                    var name = representation.ContainsKey("name") ? representation["name"].ToString() : $"Product {upc}";
                    var product = new { upc, name };
                    return new(new ResolveReferenceResult(type, product));
                },
                ["Review"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var review = new { 
                        id, 
                        body = $"Review {id}", 
                        rating = 5,
                        author = new { id = "1", username = "@reviewer1" },
                        product = new { upc = "1", name = "Product 1" }
                    };
                    return new(new ResolveReferenceResult(type, review));
                }
            }))
            .Add(new ResolversMap
            {
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "username", context => context.ResolveAsPropertyOf<dynamic>(u => u.username) },
                    { "email", context => context.ResolveAsPropertyOf<dynamic>(u => u.email) },
                    { "fullName", context => 
                        {
                            var user = (dynamic)context.ObjectValue;
                            return context.ResolveAs($"{user.username} ({user.email})");
                        }
                    }
                },
                ["Product"] = new()
                {
                    { "upc", context => context.ResolveAsPropertyOf<dynamic>(p => p.upc) },
                    { "name", context => context.ResolveAsPropertyOf<dynamic>(p => p.name) },
                    { "reviews", context => context.ResolveAs(new[] { new { id = "1", body = "Great product!" } }) }
                },
                ["Review"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(r => r.id) },
                    { "body", context => context.ResolveAsPropertyOf<dynamic>(r => r.body) },
                    { "rating", context => context.ResolveAsPropertyOf<dynamic>(r => r.rating) },
                    { "author", context => context.ResolveAsPropertyOf<dynamic>(r => r.author) },
                    { "product", context => context.ResolveAsPropertyOf<dynamic>(r => r.product) }
                }
            });

        return await builder.Build();
    }
}