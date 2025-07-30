using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests;

/// <summary>
/// Tests to verify that the Tanka GraphQL core engine produces results
/// that comply with the GraphQL specification examples and requirements.
/// 
/// These tests validate execution, validation, and type system behavior 
/// against the current GraphQL specification draft.
/// </summary>
public class SpecComplianceFacts
{
    private readonly ISchema _schema;

    public SpecComplianceFacts()
    {
        var builder = new SchemaBuilder()
            .Add(@"
                type Character {
                    id: ID!
                    name: String!
                    friends: [Character]
                }
                
                type Query {
                    hero: Character
                    character(id: ID!): Character
                }
                
                schema {
                    query: Query
                }
            ");

        var resolvers = new ResolversMap
        {
            ["Character"] = new()
            {
                { "id", context => context.ResolveAsPropertyOf<TestCharacter>(c => c.Id) },
                { "name", context => context.ResolveAsPropertyOf<TestCharacter>(c => c.Name) },
                { "friends", context => context.ResolveAsPropertyOf<TestCharacter>(c => c.Friends) }
            },
            ["Query"] = new()
            {
                { "hero", context => context.ResolveAs(GetHero()) },
                { "character", context =>
                    {
                        if (context.ArgumentValues.TryGetValue("id", out var idValue))
                        {
                            return context.ResolveAs(GetCharacter(idValue?.ToString()));
                        }
                        return context.ResolveAs(GetCharacter(null));
                    }
                }
            }
        };

        _schema = builder.Build(resolvers).Result;
    }

    private static TestCharacter GetHero()
    {
        return new TestCharacter
        {
            Id = "1",
            Name = "R2-D2",
            Friends = new[]
            {
                new TestCharacter { Id = "2", Name = "Luke Skywalker", Friends = Array.Empty<TestCharacter>() },
                new TestCharacter { Id = "3", Name = "Han Solo", Friends = Array.Empty<TestCharacter>() }
            }
        };
    }

    private static TestCharacter? GetCharacter(string? id)
    {
        return id switch
        {
            "1" => new TestCharacter { Id = "1", Name = "R2-D2", Friends = Array.Empty<TestCharacter>() },
            "2" => new TestCharacter { Id = "2", Name = "Luke Skywalker", Friends = Array.Empty<TestCharacter>() },
            "3" => new TestCharacter { Id = "3", Name = "Han Solo", Friends = Array.Empty<TestCharacter>() },
            _ => null
        };
    }

    #region Query Execution Compliance

    [Fact]
    public async Task Execute_BasicQuery_ShouldMatchSpecification()
    {
        // Given: Basic query from GraphQL specification
        var query = @"
            {
                hero {
                    name
                }
            }";

        // When: Execute the query
        var result = await Executor.Execute(_schema, query);

        // Then: Verify result matches specification format
        result.ShouldMatchJson(@"{
            ""data"": {
                ""hero"": {
                    ""name"": ""R2-D2""
                }
            }
        }");
    }

    [Fact]
    public async Task Execute_QueryWithArguments_ShouldMatchSpecification()
    {
        // Given: Query with arguments from GraphQL specification
        var query = @"
            {
                character(id: ""2"") {
                    name
                }
            }";

        // When: Execute the query
        var result = await Executor.Execute(_schema, query);

        // Then: Verify result includes requested fields
        result.ShouldMatchJson(@"{
            ""data"": {
                ""character"": {
                    ""name"": ""Luke Skywalker""
                }
            }
        }");
    }

    [Fact]
    public async Task Execute_QueryWithVariables_ShouldMatchSpecification()
    {
        // Given: Query with variables from GraphQL specification
        var query = @"
            query GetCharacter($id: ID!) {
                character(id: $id) {
                    name
                }
            }";

        var variables = new Dictionary<string, object?>
        {
            { "id", "3" }
        };

        // When: Execute the query
        var result = await Executor.Execute(_schema, query, variables);

        // Then: Verify variables are used correctly
        result.ShouldMatchJson(@"{
            ""data"": {
                ""character"": {
                    ""name"": ""Han Solo""
                }
            }
        }");
    }

    [Fact]
    public async Task Execute_QueryWithAliases_ShouldMatchSpecification()
    {
        // Given: Query with field aliases from GraphQL specification
        var query = @"
            {
                mainHero: hero {
                    name
                }
                heroAgain: hero {
                    name
                }
            }";

        // When: Execute the query
        var result = await Executor.Execute(_schema, query);

        // Then: Verify aliases in response
        result.ShouldMatchJson(@"{
            ""data"": {
                ""mainHero"": {
                    ""name"": ""R2-D2""
                },
                ""heroAgain"": {
                    ""name"": ""R2-D2""
                }
            }
        }");
    }

    [Fact]
    public async Task Execute_QueryWithFragments_ShouldMatchSpecification()
    {
        // Given: Query with fragments from GraphQL specification
        var query = @"
            {
                hero {
                    ...characterFields
                }
            }
            
            fragment characterFields on Character {
                name
                id
            }";

        // When: Execute the query
        var result = await Executor.Execute(_schema, query);

        // Then: Verify fragments are expanded correctly
        result.ShouldMatchJson(@"{
            ""data"": {
                ""hero"": {
                    ""name"": ""R2-D2"",
                    ""id"": ""1""
                }
            }
        }");
    }

    [Fact]
    public async Task Execute_QueryWithNestedFields_ShouldMatchSpecification()
    {
        // Given: Query with nested fields from GraphQL specification
        var query = @"
            {
                hero {
                    name
                    friends {
                        name
                    }
                }
            }";

        // When: Execute the query
        var result = await Executor.Execute(_schema, query);

        // Then: Verify nested fields are resolved correctly
        result.ShouldMatchJson(@"{
            ""data"": {
                ""hero"": {
                    ""name"": ""R2-D2"",
                    ""friends"": [
                        { ""name"": ""Luke Skywalker"" },
                        { ""name"": ""Han Solo"" }
                    ]
                }
            }
        }");
    }

    #endregion

    #region Validation Compliance

    [Fact]
    public async Task Validate_UnknownField_ShouldReturnError()
    {
        // Given: Query with unknown field from GraphQL specification
        var query = @"
            {
                hero {
                    unknownField
                }
            }";

        // When: Execute the invalid query
        var result = await Executor.Execute(_schema, query);

        // Then: Verify validation error is returned
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("unknownField", result.Errors[0].Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Validate_MissingRequiredArgument_ShouldReturnError()
    {
        // Given: Query missing required argument from GraphQL specification
        var query = @"
            {
                character {
                    name
                }
            }";

        // When: Execute the invalid query
        var result = await Executor.Execute(_schema, query);

        // Then: Verify validation error for missing required argument
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("id", result.Errors[0].Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Validate_WrongArgumentType_ShouldReturnError()
    {
        // Given: Query with wrong argument type from GraphQL specification
        var query = @"
            {
                character(id: 123) {
                    name
                }
            }";

        // When: Execute the invalid query
        var result = await Executor.Execute(_schema, query);

        // Then: Verify validation error for wrong argument type
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Expected", result.Errors[0].Message);
        Assert.Null(result.Data);
    }

    #endregion

    #region Directive Execution Compliance

    [Fact]
    public async Task Execute_SkipDirectiveTrue_ShouldMatchSpecification()
    {
        // Given: Query with @skip directive (true) from GraphQL specification
        var query = @"
            query Hero($skipName: Boolean!) {
                hero {
                    id
                    name @skip(if: $skipName)
                }
            }";

        var variables = new Dictionary<string, object?>
        {
            { "skipName", true }
        };

        // When: Execute the query
        var result = await Executor.Execute(_schema, query, variables);

        // Then: Verify @skip directive excludes field when true
        result.ShouldMatchJson(@"{
            ""data"": {
                ""hero"": {
                    ""id"": ""1""
                }
            }
        }");
    }

    [Fact]
    public async Task Execute_SkipDirectiveFalse_ShouldMatchSpecification()
    {
        // Given: Query with @skip directive (false) from GraphQL specification
        var query = @"
            query Hero($skipName: Boolean!) {
                hero {
                    id
                    name @skip(if: $skipName)
                }
            }";

        var variables = new Dictionary<string, object?>
        {
            { "skipName", false }
        };

        // When: Execute the query
        var result = await Executor.Execute(_schema, query, variables);

        // Then: Verify @skip directive includes field when false
        result.ShouldMatchJson(@"{
            ""data"": {
                ""hero"": {
                    ""id"": ""1"",
                    ""name"": ""R2-D2""
                }
            }
        }");
    }

    [Fact]
    public async Task Execute_IncludeDirectiveTrue_ShouldMatchSpecification()
    {
        // Given: Query with @include directive (true) from GraphQL specification
        var query = @"
            query Hero($includeName: Boolean!) {
                hero {
                    id
                    name @include(if: $includeName)
                }
            }";

        var variables = new Dictionary<string, object?>
        {
            { "includeName", true }
        };

        // When: Execute the query
        var result = await Executor.Execute(_schema, query, variables);

        // Then: Verify @include directive includes field when true
        result.ShouldMatchJson(@"{
            ""data"": {
                ""hero"": {
                    ""id"": ""1"",
                    ""name"": ""R2-D2""
                }
            }
        }");
    }

    [Fact]
    public async Task Execute_IncludeDirectiveFalse_ShouldMatchSpecification()
    {
        // Given: Query with @include directive (false) from GraphQL specification
        var query = @"
            query Hero($includeName: Boolean!) {
                hero {
                    id
                    name @include(if: $includeName)
                }
            }";

        var variables = new Dictionary<string, object?>
        {
            { "includeName", false }
        };

        // When: Execute the query
        var result = await Executor.Execute(_schema, query, variables);

        // Then: Verify @include directive excludes field when false
        result.ShouldMatchJson(@"{
            ""data"": {
                ""hero"": {
                    ""id"": ""1""
                }
            }
        }");
    }

    #endregion

    #region Introspection Compliance

    [Fact]
    public async Task Execute_TypenameIntrospection_ShouldMatchSpecification()
    {
        // Given: Query with __typename from GraphQL specification
        var query = @"
            {
                hero {
                    __typename
                    name
                }
            }";

        // When: Execute the query
        var result = await Executor.Execute(_schema, query);

        // Then: Verify __typename is included correctly
        result.ShouldMatchJson(@"{
            ""data"": {
                ""hero"": {
                    ""__typename"": ""Character"",
                    ""name"": ""R2-D2""
                }
            }
        }");
    }

    [Fact]
    public async Task Execute_SchemaIntrospection_ShouldMatchSpecification()
    {
        // Given: Schema introspection query from GraphQL specification
        var query = @"
            {
                __schema {
                    types {
                        name
                    }
                }
            }";

        // When: Execute the query
        var result = await Executor.Execute(_schema, query);

        // Then: Verify schema introspection works
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data["__schema"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Execute_TypeIntrospection_ShouldMatchSpecification()
    {
        // Given: Type introspection query from GraphQL specification
        var query = @"
            {
                __type(name: ""Character"") {
                    name
                    kind
                    fields {
                        name
                        type {
                            name
                        }
                    }
                }
            }";

        // When: Execute the query
        var result = await Executor.Execute(_schema, query);

        // Then: Verify type introspection works
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data["__type"]);
        Assert.Null(result.Errors);
    }

    #endregion
}

/// <summary>
/// Simple test character for GraphQL specification compliance tests
/// </summary>
public class TestCharacter
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public TestCharacter[] Friends { get; set; } = Array.Empty<TestCharacter>();
}