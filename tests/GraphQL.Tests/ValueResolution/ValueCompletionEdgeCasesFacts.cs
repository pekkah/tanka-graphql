using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests.ValueResolution;

/// <summary>
/// Tests for value completion edge cases, particularly interface and union type resolution,
/// circular reference detection, and complex type resolution scenarios
/// </summary>
public class ValueCompletionEdgeCasesFacts
{
    private readonly ISchema _schema;
    private readonly ValueCompletionFeature _valueCompletionFeature;

    public ValueCompletionEdgeCasesFacts()
    {
        _schema = CreateTestSchema();
        _valueCompletionFeature = new ValueCompletionFeature();
    }

    [Fact]
    public async Task CompleteInterfaceValue_WithNullResolveAbstractType_ShouldThrowFieldException()
    {
        // Given
        var context = CreateResolverContext();
        var interfaceType = _schema.GetRequiredNamedType<InterfaceDefinition>("Node");
        var value = new TestUser { Id = "1", Name = "Test User" };

        // When & Then
        var exception = await Assert.ThrowsAsync<FieldException>(() =>
            _valueCompletionFeature.CompleteValueAsync(value, interfaceType, context, new NodePath()).AsTask());

        Assert.Contains("ActualType is required for interface values", exception.Message);
    }

    [Fact]
    public async Task CompleteInterfaceValue_WithInvalidActualType_ShouldThrowFieldException()
    {
        // Given
        var context = CreateResolverContext();
        context.ResolveAbstractType = (_, _) => _schema.GetRequiredNamedType<ObjectDefinition>("InvalidType");
        var interfaceType = _schema.GetRequiredNamedType<InterfaceDefinition>("Node");
        var value = new TestUser { Id = "1", Name = "Test User" };

        // When & Then
        var exception = await Assert.ThrowsAsync<FieldException>(() =>
            _valueCompletionFeature.CompleteValueAsync(value, interfaceType, context, new NodePath()).AsTask());

        Assert.Contains("does not implement interface", exception.Message);
    }

    [Fact]
    public async Task CompleteInterfaceValue_WithValidActualType_ShouldCompleteSuccessfully()
    {
        // Given
        var context = CreateResolverContext();
        var userType = _schema.GetRequiredNamedType<ObjectDefinition>("User");
        context.ResolveAbstractType = (_, _) => userType;
        var interfaceType = _schema.GetRequiredNamedType<InterfaceDefinition>("Node");
        var value = new TestUser { Id = "1", Name = "Test User" };

        // When
        var result = await _valueCompletionFeature.CompleteValueAsync(value, interfaceType, context, new NodePath());

        // Then
        Assert.NotNull(result);
        Assert.IsType<Dictionary<string, object?>>(result);
    }

    [Fact]
    public async Task CompleteUnionValue_WithNullResolveAbstractType_ShouldThrowFieldException()
    {
        // Given
        var context = CreateResolverContext();
        var unionType = _schema.GetRequiredNamedType<UnionDefinition>("SearchResult");
        var value = new TestUser { Id = "1", Name = "Test User" };

        // When & Then
        var exception = await Assert.ThrowsAsync<FieldException>(() =>
            _valueCompletionFeature.CompleteValueAsync(value, unionType, context, new NodePath()).AsTask());

        Assert.Contains("ActualType is required for union values", exception.Message);
    }

    [Fact]
    public async Task CompleteUnionValue_WithInvalidUnionMember_ShouldThrowFieldException()
    {
        // Given
        var context = CreateResolverContext();
        context.ResolveAbstractType = (_, _) => _schema.GetRequiredNamedType<ObjectDefinition>("InvalidType");
        var unionType = _schema.GetRequiredNamedType<UnionDefinition>("SearchResult");
        var value = new TestUser { Id = "1", Name = "Test User" };

        // When & Then
        var exception = await Assert.ThrowsAsync<FieldException>(() =>
            _valueCompletionFeature.CompleteValueAsync(value, unionType, context, new NodePath()).AsTask());

        Assert.Contains("is not possible for", exception.Message);
    }

    [Fact]
    public async Task CompleteUnionValue_WithValidUnionMember_ShouldCompleteSuccessfully()
    {
        // Given
        var context = CreateResolverContext();
        var userType = _schema.GetRequiredNamedType<ObjectDefinition>("User");
        context.ResolveAbstractType = (_, _) => userType;
        var unionType = _schema.GetRequiredNamedType<UnionDefinition>("SearchResult");
        var value = new TestUser { Id = "1", Name = "Test User" };

        // When
        var result = await _valueCompletionFeature.CompleteValueAsync(value, unionType, context, new NodePath());

        // Then
        Assert.NotNull(result);
        Assert.IsType<Dictionary<string, object?>>(result);
    }

    [Fact]
    public async Task CompleteListValue_WithNonEnumerableValue_ShouldThrowFieldException()
    {
        // Given
        var context = CreateResolverContext();
        var listType = new ListType(new NamedType("String"));
        var value = "not a list";

        // When & Then
        var exception = await Assert.ThrowsAsync<FieldException>(() =>
            _valueCompletionFeature.CompleteValueAsync(value, listType, context, new NodePath()).AsTask());

        Assert.Contains("Resolved value is not a collection", exception.Message);
    }

    [Fact]
    public async Task CompleteListValue_WithNullItemsInNonNullList_ShouldThrowFieldException()
    {
        // Given
        var context = CreateResolverContext();
        var listType = new ListType(new NonNullType(new NamedType("String")));
        var value = new List<string?> { "item1", null, "item3" };

        // When & Then
        var exception = await Assert.ThrowsAsync<FieldException>(() =>
            _valueCompletionFeature.CompleteValueAsync(value, listType, context, new NodePath()).AsTask());

        Assert.Contains("Completed value would be null for non-null field", exception.Message);
    }

    [Fact]
    public async Task CompleteListValue_WithNullItemsInNullableList_ShouldHandleGracefully()
    {
        // Given
        var context = CreateResolverContext();
        var listType = new ListType(new NamedType("String"));
        var value = new List<string?> { "item1", null, "item3" };

        // When
        var result = await _valueCompletionFeature.CompleteValueAsync(value, listType, context, new NodePath());

        // Then
        Assert.NotNull(result);
        var resultList = Assert.IsType<List<object?>>(result);
        Assert.Equal(3, resultList.Count);
        Assert.Equal("item1", resultList[0]);
        Assert.Null(resultList[1]);
        Assert.Equal("item3", resultList[2]);
    }

    [Fact]
    public async Task CompleteNonNullValue_WithNullValue_ShouldThrowFieldException()
    {
        // Given
        var context = CreateResolverContext();
        var nonNullType = new NonNullType(new NamedType("String"));
        object? value = null;

        // When & Then
        var exception = await Assert.ThrowsAsync<FieldException>(() =>
            _valueCompletionFeature.CompleteValueAsync(value, nonNullType, context, new NodePath()).AsTask());

        Assert.Contains("Completed value would be null for non-null field", exception.Message);
    }

    [Fact]
    public async Task CompleteValue_WithCircularReference_ShouldHandleGracefully()
    {
        // Given
        var context = CreateResolverContext();
        var userType = _schema.GetRequiredNamedType<ObjectDefinition>("User");
        
        // Create circular reference
        var user1 = new TestUser { Id = "1", Name = "User 1" };
        var user2 = new TestUser { Id = "2", Name = "User 2" };
        user1.Friend = user2;
        user2.Friend = user1;

        // When
        var result = await _valueCompletionFeature.CompleteValueAsync(user1, userType, context, new NodePath());

        // Then
        Assert.NotNull(result);
        // The circular reference should be handled by the execution engine
        Assert.IsType<Dictionary<string, object?>>(result);
    }

    [Fact]
    public async Task CompleteValue_WithDeepNesting_ShouldHandleCorrectly()
    {
        // Given
        var context = CreateResolverContext();
        var userType = _schema.GetRequiredNamedType<ObjectDefinition>("User");
        
        // Create deep nesting
        var rootUser = new TestUser { Id = "1", Name = "Root User" };
        var currentUser = rootUser;
        
        for (int i = 2; i <= 50; i++)
        {
            var nextUser = new TestUser { Id = i.ToString(), Name = $"User {i}" };
            currentUser.Friend = nextUser;
            currentUser = nextUser;
        }

        // When
        var result = await _valueCompletionFeature.CompleteValueAsync(rootUser, userType, context, new NodePath());

        // Then
        Assert.NotNull(result);
        Assert.IsType<Dictionary<string, object?>>(result);
    }

    [Fact]
    public async Task CompleteValue_WithInvalidTypeDefinition_ShouldThrowFieldException()
    {
        // Given
        var context = CreateResolverContext();
        var invalidType = new NamedType("NonExistentType");
        var value = "test value";

        // When & Then
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _valueCompletionFeature.CompleteValueAsync(value, invalidType, context, new NodePath()).AsTask());

        Assert.Contains("Type 'NonExistentType' not found", exception.Message);
    }

    [Fact]
    public async Task CompleteValue_WithUnsupportedTypeDefinition_ShouldThrowFieldException()
    {
        // Given
        var context = CreateResolverContext();
        // Create a custom type that's not supported
        var unsupportedType = new NamedType("InputType");
        var value = "test value";

        // When & Then
        var exception = await Assert.ThrowsAsync<FieldException>(() =>
            _valueCompletionFeature.CompleteValueAsync(value, unsupportedType, context, new NodePath()).AsTask());

        Assert.Contains("Cannot complete value of type", exception.Message);
    }

    [Fact]
    public async Task CompleteValue_WithComplexNestedStructure_ShouldHandleCorrectly()
    {
        // Given
        var context = CreateResolverContext();
        var listType = new ListType(new NamedType("User"));
        var users = new List<TestUser>
        {
            new() { Id = "1", Name = "User 1" },
            new() { Id = "2", Name = "User 2" },
            new() { Id = "3", Name = "User 3" }
        };

        // When
        var result = await _valueCompletionFeature.CompleteValueAsync(users, listType, context, new NodePath());

        // Then
        Assert.NotNull(result);
        var resultList = Assert.IsType<List<object?>>(result);
        Assert.Equal(3, resultList.Count);
        Assert.All(resultList, item => Assert.IsType<Dictionary<string, object?>>(item));
    }

    private ResolverContext CreateResolverContext()
    {
        var objectDefinition = _schema.GetRequiredNamedType<ObjectDefinition>("User");
        var field = objectDefinition.GetField("name");
        var fieldSelection = new FieldSelection(new Name("name"));

        return new ResolverContext
        {
            ObjectDefinition = objectDefinition,
            Field = field,
            Selection = fieldSelection,
            Fields = new List<FieldSelection> { fieldSelection },
            Path = new NodePath(),
            QueryContext = new QueryContext(_schema, new GraphQLRequest { Query = "{ name }" })
        };
    }

    private static ISchema CreateTestSchema()
    {
        var schemaBuilder = new SchemaBuilder();
        schemaBuilder.Add(@"
            interface Node {
                id: ID!
            }

            type User implements Node {
                id: ID!
                name: String!
                friend: User
            }

            type Post implements Node {
                id: ID!
                title: String!
                author: User!
            }

            union SearchResult = User | Post

            type InvalidType {
                id: ID!
                name: String!
            }

            input TestInput {
                name: String!
            }

            type Query {
                user(id: ID!): User
                search(query: String!): [SearchResult!]!
                node(id: ID!): Node
            }

            schema {
                query: Query
            }
        ");

        var resolvers = new ResolversMap
        {
            ["User"] = new FieldResolversMap
            {
                ["id"] = context => context.ResolveAsPropertyOf<TestUser>(u => u.Id),
                ["name"] = context => context.ResolveAsPropertyOf<TestUser>(u => u.Name),
                ["friend"] = context => context.ResolveAsPropertyOf<TestUser>(u => u.Friend)
            },
            ["Post"] = new FieldResolversMap
            {
                ["id"] = context => context.ResolveAsPropertyOf<TestPost>(p => p.Id),
                ["title"] = context => context.ResolveAsPropertyOf<TestPost>(p => p.Title),
                ["author"] = context => context.ResolveAsPropertyOf<TestPost>(p => p.Author)
            },
            ["Query"] = new FieldResolversMap
            {
                ["user"] = context => context.ResolveAs(new TestUser { Id = "1", Name = "Test User" }),
                ["search"] = context => context.ResolveAs(new List<object> { new TestUser { Id = "1", Name = "Test User" } }),
                ["node"] = context => context.ResolveAs(new TestUser { Id = "1", Name = "Test User" })
            }
        };

        return schemaBuilder.Build(resolvers).Result;
    }

    public class TestUser
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TestUser? Friend { get; set; }
    }

    public class TestPost
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public TestUser Author { get; set; } = new();
    }
}