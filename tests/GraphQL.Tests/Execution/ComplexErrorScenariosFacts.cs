using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Tests.Execution;

/// <summary>
/// Tests for complex error scenarios involving multiple simultaneous errors,
/// error propagation, and error handling in nested structures
/// </summary>
public class ComplexErrorScenariosFacts
{
    private readonly ISchema _schema;
    private readonly ResolversMap _resolvers;

    public ComplexErrorScenariosFacts()
    {
        (_schema, _resolvers) = CreateTestSchema();
    }

    [Fact]
    public async Task Execute_WithMultipleFieldErrors_ShouldCollectAllErrors()
    {
        // Given
        var query = @"
            {
                user(id: ""1"") {
                    id
                    name
                    errorField1
                    errorField2
                    errorField3
                }
            }";

        // When
        var result = await Executor.Execute(_schema, query);

        // Then
        Assert.NotNull(result.Errors);
        Assert.Equal(3, result.Errors.Count);
        
        var errorMessages = result.Errors.Select(e => e.Message).ToList();
        Assert.Contains("Error in field 1", errorMessages);
        Assert.Contains("Error in field 2", errorMessages);
        Assert.Contains("Error in field 3", errorMessages);
        
        // Data should still be present for non-error fields
        Assert.NotNull(result.Data);
        var userData = result.Data["user"] as Dictionary<string, object?>;
        Assert.NotNull(userData);
        Assert.Equal("1", userData["id"]);
        Assert.Equal("Test User", userData["name"]);
    }

    [Fact]
    public async Task Execute_WithNestedFieldErrors_ShouldHandleErrorPropagation()
    {
        // Given
        var query = @"
            {
                user(id: ""1"") {
                    id
                    name
                    profile {
                        bio
                        errorField
                        settings {
                            theme
                            errorField
                        }
                    }
                }
            }";

        // When
        var result = await Executor.Execute(_schema, query);

        // Then
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors.Count);
        
        // Check error paths
        var error1 = result.Errors.FirstOrDefault(e => e.Path.SequenceEqual(new object[] { "user", "profile", "errorField" }));
        var error2 = result.Errors.FirstOrDefault(e => e.Path.SequenceEqual(new object[] { "user", "profile", "settings", "errorField" }));
        
        Assert.NotNull(error1);
        Assert.NotNull(error2);
        
        // Data should still be present for non-error fields
        Assert.NotNull(result.Data);
        var userData = result.Data["user"] as Dictionary<string, object?>;
        Assert.NotNull(userData);
        var profileData = userData["profile"] as Dictionary<string, object?>;
        Assert.NotNull(profileData);
        Assert.Equal("User bio", profileData["bio"]);
    }

    [Fact]
    public async Task Execute_WithListFieldErrors_ShouldHandlePartialErrors()
    {
        // Given
        var query = @"
            {
                users {
                    id
                    name
                    errorField
                }
            }";

        // When
        var result = await Executor.Execute(_schema, query);

        // Then
        Assert.NotNull(result.Errors);
        Assert.Equal(3, result.Errors.Count); // 3 users, each with an error
        
        // Check that all errors have correct paths
        for (int i = 0; i < 3; i++)
        {
            var error = result.Errors.FirstOrDefault(e => e.Path.SequenceEqual(new object[] { "users", i, "errorField" }));
            Assert.NotNull(error);
            Assert.Equal("Error in field", error.Message);
        }
        
        // Data should still be present for non-error fields
        Assert.NotNull(result.Data);
        var usersData = result.Data["users"] as List<object>;
        Assert.NotNull(usersData);
        Assert.Equal(3, usersData.Count);
    }

    [Fact]
    public async Task Execute_WithNonNullFieldErrors_ShouldPropagate()
    {
        // Given
        var query = @"
            {
                user(id: ""1"") {
                    id
                    name
                    nonNullErrorField
                }
            }";

        // When
        var result = await Executor.Execute(_schema, query);

        // Then
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        
        // The error should cause the parent object to be null
        Assert.NotNull(result.Data);
        Assert.Null(result.Data["user"]);
    }

    [Fact]
    public async Task Execute_WithMixedErrorTypes_ShouldHandleDifferentExceptions()
    {
        // Given
        var query = @"
            {
                user(id: ""1"") {
                    id
                    name
                    argumentError
                    validationError
                    customError
                }
            }";

        // When
        var result = await Executor.Execute(_schema, query);

        // Then
        Assert.NotNull(result.Errors);
        Assert.Equal(3, result.Errors.Count);
        
        var errorMessages = result.Errors.Select(e => e.Message).ToList();
        Assert.Contains("Argument error", errorMessages);
        Assert.Contains("Validation error", errorMessages);
        Assert.Contains("Custom error", errorMessages);
    }

    [Fact]
    public async Task Execute_WithErrorsInArrayElements_ShouldHandleCorrectly()
    {
        // Given
        var query = @"
            {
                user(id: ""1"") {
                    id
                    name
                    posts {
                        id
                        title
                        errorField
                    }
                }
            }";

        // When
        var result = await Executor.Execute(_schema, query);

        // Then
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors.Count); // 2 posts, each with an error
        
        // Check error paths
        var error1 = result.Errors.FirstOrDefault(e => e.Path.SequenceEqual(new object[] { "user", "posts", 0, "errorField" }));
        var error2 = result.Errors.FirstOrDefault(e => e.Path.SequenceEqual(new object[] { "user", "posts", 1, "errorField" }));
        
        Assert.NotNull(error1);
        Assert.NotNull(error2);
        
        // Data should still be present for non-error fields
        Assert.NotNull(result.Data);
        var userData = result.Data["user"] as Dictionary<string, object?>;
        Assert.NotNull(userData);
        var postsData = userData["posts"] as List<object>;
        Assert.NotNull(postsData);
        Assert.Equal(2, postsData.Count);
    }

    [Fact]
    public async Task Execute_WithCircularReferenceAndErrors_ShouldHandleGracefully()
    {
        // Given
        var query = @"
            {
                user(id: ""1"") {
                    id
                    name
                    friend {
                        id
                        name
                        errorField
                        friend {
                            id
                            name
                            errorField
                        }
                    }
                }
            }";

        // When
        var result = await Executor.Execute(_schema, query);

        // Then
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors.Count);
        
        // Check error paths
        var error1 = result.Errors.FirstOrDefault(e => e.Path.SequenceEqual(new object[] { "user", "friend", "errorField" }));
        var error2 = result.Errors.FirstOrDefault(e => e.Path.SequenceEqual(new object[] { "user", "friend", "friend", "errorField" }));
        
        Assert.NotNull(error1);
        Assert.NotNull(error2);
    }

    [Fact]
    public async Task Execute_WithErrorsInUnionTypes_ShouldHandleCorrectly()
    {
        // Given
        var query = @"
            {
                searchResults {
                    __typename
                    ... on User {
                        id
                        name
                        errorField
                    }
                    ... on Post {
                        id
                        title
                        errorField
                    }
                }
            }";

        // When
        var result = await Executor.Execute(_schema, query);

        // Then
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors.Count); // One user and one post, each with an error
        
        // Check error paths
        var userError = result.Errors.FirstOrDefault(e => e.Path.SequenceEqual(new object[] { "searchResults", 0, "errorField" }));
        var postError = result.Errors.FirstOrDefault(e => e.Path.SequenceEqual(new object[] { "searchResults", 1, "errorField" }));
        
        Assert.NotNull(userError);
        Assert.NotNull(postError);
    }

    [Fact]
    public async Task Execute_WithErrorsInInterfaceTypes_ShouldHandleCorrectly()
    {
        // Given
        var query = @"
            {
                nodes {
                    id
                    errorField
                    ... on User {
                        name
                    }
                    ... on Post {
                        title
                    }
                }
            }";

        // When
        var result = await Executor.Execute(_schema, query);

        // Then
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors.Count); // One user and one post, each with an error
        
        // Check error paths
        var userError = result.Errors.FirstOrDefault(e => e.Path.SequenceEqual(new object[] { "nodes", 0, "errorField" }));
        var postError = result.Errors.FirstOrDefault(e => e.Path.SequenceEqual(new object[] { "nodes", 1, "errorField" }));
        
        Assert.NotNull(userError);
        Assert.NotNull(postError);
    }

    [Fact]
    public async Task Execute_WithAsyncErrorsInParallel_ShouldHandleCorrectly()
    {
        // Given
        var query = @"
            {
                user(id: ""1"") {
                    id
                    name
                    asyncError1
                    asyncError2
                    asyncError3
                }
            }";

        // When
        var result = await Executor.Execute(_schema, query);

        // Then
        Assert.NotNull(result.Errors);
        Assert.Equal(3, result.Errors.Count);
        
        var errorMessages = result.Errors.Select(e => e.Message).ToList();
        Assert.Contains("Async error 1", errorMessages);
        Assert.Contains("Async error 2", errorMessages);
        Assert.Contains("Async error 3", errorMessages);
    }

    [Fact]
    public async Task Execute_WithErrorsInVariableCoercion_ShouldHandleCorrectly()
    {
        // Given
        var query = @"
            query GetUser($id: ID!) {
                user(id: $id) {
                    id
                    name
                }
            }";

        var variables = new Dictionary<string, object>
        {
            ["id"] = new { invalid = "object" } // Invalid variable type
        };

        // When
        var result = await Executor.Execute(_schema, query, variables);

        // Then
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task Execute_WithMaxDepthExceeded_ShouldHandleGracefully()
    {
        // Given - Create a deeply nested query
        var query = @"
            {
                user(id: ""1"") {
                    friend {
                        friend {
                            friend {
                                friend {
                                    friend {
                                        friend {
                                            friend {
                                                friend {
                                                    friend {
                                                        friend {
                                                            id
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }";

        // When
        var result = await Executor.Execute(_schema, query);

        // Then
        // Should complete without throwing (may have performance implications but should not crash)
        Assert.NotNull(result);
    }

    private static (ISchema, ResolversMap) CreateTestSchema()
    {
        var schemaBuilder = new SchemaBuilder();
        schemaBuilder.Add(@"
            interface Node {
                id: ID!
                errorField: String
            }

            type User implements Node {
                id: ID!
                name: String!
                errorField: String
                errorField1: String
                errorField2: String
                errorField3: String
                nonNullErrorField: String!
                argumentError: String
                validationError: String
                customError: String
                asyncError1: String
                asyncError2: String
                asyncError3: String
                profile: Profile
                friend: User
                posts: [Post!]!
            }

            type Post implements Node {
                id: ID!
                title: String!
                errorField: String
                author: User!
            }

            type Profile {
                bio: String!
                errorField: String
                settings: Settings!
            }

            type Settings {
                theme: String!
                errorField: String
            }

            union SearchResult = User | Post

            type Query {
                user(id: ID!): User
                users: [User!]!
                searchResults: [SearchResult!]!
                nodes: [Node!]!
            }

            schema {
                query: Query
            }
        ");

        var testUsers = new List<TestUser>
        {
            new() { Id = "1", Name = "Test User 1", Friend = new TestUser { Id = "2", Name = "Test User 2" } },
            new() { Id = "2", Name = "Test User 2", Friend = new TestUser { Id = "1", Name = "Test User 1" } },
            new() { Id = "3", Name = "Test User 3" }
        };

        var testPosts = new List<TestPost>
        {
            new() { Id = "1", Title = "Test Post 1", Author = testUsers[0] },
            new() { Id = "2", Title = "Test Post 2", Author = testUsers[1] }
        };

        testUsers[0].Posts = testPosts.Take(2).ToList();
        testUsers[1].Posts = new List<TestPost>();
        testUsers[2].Posts = new List<TestPost>();

        var resolvers = new ResolversMap
        {
            ["User"] = new FieldResolversMap
            {
                ["id"] = context => context.ResolveAsPropertyOf<TestUser>(u => u.Id),
                ["name"] = context => context.ResolveAsPropertyOf<TestUser>(u => u.Name),
                ["errorField"] = context => throw new InvalidOperationException("Error in field"),
                ["errorField1"] = context => throw new InvalidOperationException("Error in field 1"),
                ["errorField2"] = context => throw new InvalidOperationException("Error in field 2"),
                ["errorField3"] = context => throw new InvalidOperationException("Error in field 3"),
                ["nonNullErrorField"] = context => throw new InvalidOperationException("Non-null error"),
                ["argumentError"] = context => throw new ArgumentException("Argument error"),
                ["validationError"] = context => throw new InvalidOperationException("Validation error"),
                ["customError"] = context => throw new CustomException("Custom error"),
                ["asyncError1"] = async context => { await Task.Delay(10); throw new InvalidOperationException("Async error 1"); },
                ["asyncError2"] = async context => { await Task.Delay(20); throw new InvalidOperationException("Async error 2"); },
                ["asyncError3"] = async context => { await Task.Delay(30); throw new InvalidOperationException("Async error 3"); },
                ["profile"] = context => context.ResolveAs(new TestProfile { Bio = "User bio", Settings = new TestSettings { Theme = "dark" } }),
                ["friend"] = context => context.ResolveAsPropertyOf<TestUser>(u => u.Friend),
                ["posts"] = context => context.ResolveAsPropertyOf<TestUser>(u => u.Posts)
            },
            ["Post"] = new FieldResolversMap
            {
                ["id"] = context => context.ResolveAsPropertyOf<TestPost>(p => p.Id),
                ["title"] = context => context.ResolveAsPropertyOf<TestPost>(p => p.Title),
                ["errorField"] = context => throw new InvalidOperationException("Error in field"),
                ["author"] = context => context.ResolveAsPropertyOf<TestPost>(p => p.Author)
            },
            ["Profile"] = new FieldResolversMap
            {
                ["bio"] = context => context.ResolveAsPropertyOf<TestProfile>(p => p.Bio),
                ["errorField"] = context => throw new InvalidOperationException("Error in profile field"),
                ["settings"] = context => context.ResolveAsPropertyOf<TestProfile>(p => p.Settings)
            },
            ["Settings"] = new FieldResolversMap
            {
                ["theme"] = context => context.ResolveAsPropertyOf<TestSettings>(s => s.Theme),
                ["errorField"] = context => throw new InvalidOperationException("Error in settings field")
            },
            ["Query"] = new FieldResolversMap
            {
                ["user"] = context => context.ResolveAs(testUsers.First()),
                ["users"] = context => context.ResolveAs(testUsers),
                ["searchResults"] = context => context.ResolveAs(new List<object> { testUsers[0], testPosts[0] }),
                ["nodes"] = context => context.ResolveAs(new List<object> { testUsers[0], testPosts[0] })
            }
        };

        var schema = schemaBuilder.Build(resolvers).Result;
        return (schema, resolvers);
    }

    public class TestUser
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TestUser? Friend { get; set; }
        public List<TestPost> Posts { get; set; } = new();
    }

    public class TestPost
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public TestUser Author { get; set; } = new();
    }

    public class TestProfile
    {
        public string Bio { get; set; } = string.Empty;
        public TestSettings Settings { get; set; } = new();
    }

    public class TestSettings
    {
        public string Theme { get; set; } = string.Empty;
    }

    public class CustomException : Exception
    {
        public CustomException(string message) : base(message) { }
    }
}