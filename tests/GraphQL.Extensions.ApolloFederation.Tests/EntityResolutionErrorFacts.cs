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

public class EntityResolutionErrorFacts
{
    [Fact]
    public async Task Query_entities_missing_typename_throws_error()
    {
        // Given
        var schema = await CreateTestSchema();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ...on User {
                                id
                                name
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
                            // Missing __typename
                            ["id"] = 1
                        }
                    }
                }
            });

        // Then
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Typename not found for representation", result.Errors[0].Message);
    }

    [Fact]
    public async Task Query_entities_null_typename_throws_error()
    {
        // Given
        var schema = await CreateTestSchema();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ...on User {
                                id
                                name
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
                            ["__typename"] = null,
                            ["id"] = 1
                        }
                    }
                }
            });

        // Then
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Representation is missing __typename", result.Errors[0].Message);
    }

    [Fact]
    public async Task Query_entities_unknown_typename_throws_error()
    {
        // Given
        var schema = await CreateTestSchema();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ...on User {
                                id
                                name
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
                            ["__typename"] = "UnknownType",
                            ["id"] = 1
                        }
                    }
                }
            });

        // Then
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Could not resolve type from __typename: 'UnknownType'", result.Errors[0].Message);
    }

    [Fact]
    public async Task Query_entities_missing_reference_resolver_throws_error()
    {
        // Given
        var schema = await CreateTestSchemaWithoutReferenceResolver();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ...on User {
                                id
                                name
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
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Could not find reference resolvers for  __typename: 'User'", result.Errors[0].Message);
    }

    [Fact]
    public async Task Query_entities_reference_resolver_throws_exception()
    {
        // Given
        var schema = await CreateTestSchemaWithFailingReferenceResolver();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ...on User {
                                id
                                name
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
                            ["id"] = 999 // ID that will cause the resolver to fail
                        }
                    }
                }
            });

        // Then
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Reference resolver failed for User", result.Errors[0].Message);
    }

    [Fact]
    public async Task Query_entities_empty_representations_returns_empty_array()
    {
        // Given
        var schema = await CreateTestSchema();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ...on User {
                                id
                                name
                            }
                        }
                    }
                    """,
                Variables = new Dictionary<string, object>
                {
                    ["representations"] = new object[0]
                }
            });

        // Then
        Assert.Null(result.Errors);
        result.ShouldMatchJson(@"{
            ""data"": {
                ""_entities"": []
            },
            ""extensions"": null,
            ""errors"": null
        }");
    }

    [Fact]
    public async Task Query_entities_multiple_representations_with_one_invalid_returns_partial_error()
    {
        // Given
        var schema = await CreateTestSchema();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ...on User {
                                id
                                name
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
                        },
                        new Dictionary<string, object>
                        {
                            // Missing __typename - this should cause an error
                            ["id"] = 2
                        }
                    }
                }
            });

        // Then
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Typename not found for representation", result.Errors[0].Message);
    }

    private static async Task<ISchema> CreateTestSchema()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") {
                    id: ID!
                    name: String!
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
                    var user = new { id, name = $"User {id}" };
                    return new(new ResolveReferenceResult(type, user));
                }
            }))
            .Add(new ResolversMap
            {
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "name", context => context.ResolveAsPropertyOf<dynamic>(u => u.name) }
                }
            });

        return await builder.Build();
    }

    private static async Task<ISchema> CreateTestSchemaWithoutReferenceResolver()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") {
                    id: ID!
                    name: String!
                }

                type Query {
                    # Empty query type
                }
            ")
            .AddSubgraph(new(new DictionaryReferenceResolversMap
            {
                // Intentionally not adding User reference resolver
            }))
            .Add(new ResolversMap
            {
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "name", context => context.ResolveAsPropertyOf<dynamic>(u => u.name) }
                }
            });

        return await builder.Build();
    }

    private static async Task<ISchema> CreateTestSchemaWithFailingReferenceResolver()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") {
                    id: ID!
                    name: String!
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
                    if (id == "999")
                    {
                        throw new InvalidOperationException("Reference resolver failed for User");
                    }
                    var user = new { id, name = $"User {id}" };
                    return new(new ResolveReferenceResult(type, user));
                }
            }))
            .Add(new ResolversMap
            {
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "name", context => context.ResolveAsPropertyOf<dynamic>(u => u.name) }
                }
            });

        return await builder.Build();
    }
}