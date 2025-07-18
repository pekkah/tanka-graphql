using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

public class ComplexEntityScenarioFacts
{
    [Fact]
    public async Task Query_multiple_different_entity_types_in_single_request()
    {
        // Given
        var schema = await CreateMultiEntitySchema();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ... on User {
                                id
                                username
                                __typename
                            }
                            ... on Product {
                                upc
                                name
                                __typename
                            }
                            ... on Review {
                                id
                                body
                                rating
                                __typename
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
                            ["id"] = "1"
                        },
                        new Dictionary<string, object>
                        {
                            ["__typename"] = "Product",
                            ["upc"] = "100"
                        },
                        new Dictionary<string, object>
                        {
                            ["__typename"] = "Review",
                            ["id"] = "1001"
                        }
                    }
                }
            });

        // Then
        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
        var entities = result.Data["_entities"] as object[];
        Assert.NotNull(entities);
        Assert.Equal(3, entities.Length);
        
        result.ShouldMatchJson(@"{
            ""data"": {
                ""_entities"": [
                    {
                        ""id"": ""1"",
                        ""username"": ""@user1"",
                        ""__typename"": ""User""
                    },
                    {
                        ""upc"": ""100"",
                        ""name"": ""Product 100"",
                        ""__typename"": ""Product""
                    },
                    {
                        ""id"": ""1001"",
                        ""body"": ""Review 1001"",
                        ""rating"": 5,
                        ""__typename"": ""Review""
                    }
                ]
            },
            ""extensions"": null,
            ""errors"": null
        }");
    }

    [Fact]
    public async Task Query_multiple_instances_of_same_entity_type()
    {
        // Given
        var schema = await CreateMultiEntitySchema();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ... on User {
                                id
                                username
                                __typename
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
                            ["id"] = "1"
                        },
                        new Dictionary<string, object>
                        {
                            ["__typename"] = "User",
                            ["id"] = "2"
                        },
                        new Dictionary<string, object>
                        {
                            ["__typename"] = "User",
                            ["id"] = "3"
                        }
                    }
                }
            });

        // Then
        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
        var entities = result.Data["_entities"] as object[];
        Assert.NotNull(entities);
        Assert.Equal(3, entities.Length);
        
        result.ShouldMatchJson(@"{
            ""data"": {
                ""_entities"": [
                    {
                        ""id"": ""1"",
                        ""username"": ""@user1"",
                        ""__typename"": ""User""
                    },
                    {
                        ""id"": ""2"",
                        ""username"": ""@user2"",
                        ""__typename"": ""User""
                    },
                    {
                        ""id"": ""3"",
                        ""username"": ""@user3"",
                        ""__typename"": ""User""
                    }
                ]
            },
            ""extensions"": null,
            ""errors"": null
        }");
    }

    [Fact]
    public async Task Query_nested_entity_relationships()
    {
        // Given
        var schema = await CreateNestedEntitySchema();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ... on User {
                                id
                                username
                                reviews {
                                    id
                                    body
                                    product {
                                        upc
                                        name
                                        category {
                                            id
                                            name
                                        }
                                    }
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
                            ["__typename"] = "User",
                            ["id"] = "1"
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
        
        // Verify nested structure exists
        var user = entities[0] as Dictionary<string, object>;
        Assert.NotNull(user);
        Assert.Contains("reviews", user);
        var reviews = user["reviews"] as object[];
        Assert.NotNull(reviews);
        Assert.True(reviews.Length > 0);
    }

    [Fact]
    public async Task Query_entities_with_circular_references()
    {
        // Given
        var schema = await CreateCircularReferenceSchema();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ... on User {
                                id
                                username
                                friends {
                                    id
                                    username
                                    friends {
                                        id
                                        username
                                    }
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
                            ["__typename"] = "User",
                            ["id"] = "1"
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
        
        // Verify circular reference structure exists
        var user = entities[0] as Dictionary<string, object>;
        Assert.NotNull(user);
        Assert.Contains("friends", user);
        var friends = user["friends"] as object[];
        Assert.NotNull(friends);
        Assert.True(friends.Length > 0);
    }

    [Fact]
    public async Task Query_entities_with_deep_nesting()
    {
        // Given
        var schema = await CreateDeeplyNestedSchema();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ... on Organization {
                                id
                                name
                                departments {
                                    id
                                    name
                                    teams {
                                        id
                                        name
                                        members {
                                            id
                                            username
                                            role
                                        }
                                    }
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
                            ["__typename"] = "Organization",
                            ["id"] = "1"
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
        
        // Verify deep nesting structure exists
        var org = entities[0] as Dictionary<string, object>;
        Assert.NotNull(org);
        Assert.Contains("departments", org);
        var departments = org["departments"] as object[];
        Assert.NotNull(departments);
        Assert.True(departments.Length > 0);
    }

    [Fact]
    public async Task Query_entities_with_large_result_set()
    {
        // Given
        var schema = await CreateLargeResultSetSchema();

        // When
        var result = await new Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ... on User {
                                id
                                username
                                posts {
                                    id
                                    title
                                    comments {
                                        id
                                        body
                                        author {
                                            id
                                            username
                                        }
                                    }
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
                            ["__typename"] = "User",
                            ["id"] = "1"
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
        
        // Verify large result set structure exists
        var user = entities[0] as Dictionary<string, object>;
        Assert.NotNull(user);
        Assert.Contains("posts", user);
        var posts = user["posts"] as object[];
        Assert.NotNull(posts);
        Assert.True(posts.Length >= 10); // Expect at least 10 posts
    }

    private static async Task<ISchema> CreateMultiEntitySchema()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") {
                    id: ID!
                    username: String!
                }

                type Product @key(fields: ""upc"") {
                    upc: String!
                    name: String!
                }

                type Review @key(fields: ""id"") {
                    id: ID!
                    body: String!
                    rating: Int!
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
                ["Product"] = (context, type, representation) =>
                {
                    var upc = representation["upc"].ToString();
                    var product = new { upc, name = $"Product {upc}" };
                    return new(new ResolveReferenceResult(type, product));
                },
                ["Review"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var review = new { id, body = $"Review {id}", rating = 5 };
                    return new(new ResolveReferenceResult(type, review));
                }
            }))
            .Add(new ResolversMap
            {
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "username", context => context.ResolveAsPropertyOf<dynamic>(u => u.username) }
                },
                ["Product"] = new()
                {
                    { "upc", context => context.ResolveAsPropertyOf<dynamic>(p => p.upc) },
                    { "name", context => context.ResolveAsPropertyOf<dynamic>(p => p.name) }
                },
                ["Review"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(r => r.id) },
                    { "body", context => context.ResolveAsPropertyOf<dynamic>(r => r.body) },
                    { "rating", context => context.ResolveAsPropertyOf<dynamic>(r => r.rating) }
                }
            });

        return await builder.Build();
    }

    private static async Task<ISchema> CreateNestedEntitySchema()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") {
                    id: ID!
                    username: String!
                    reviews: [Review!]!
                }

                type Product @key(fields: ""upc"") {
                    upc: String!
                    name: String!
                    category: Category
                }

                type Review @key(fields: ""id"") {
                    id: ID!
                    body: String!
                    product: Product
                }

                type Category @key(fields: ""id"") {
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
                    var user = new { id, username = $"@user{id}" };
                    return new(new ResolveReferenceResult(type, user));
                },
                ["Product"] = (context, type, representation) =>
                {
                    var upc = representation["upc"].ToString();
                    var product = new { upc, name = $"Product {upc}", category = new { id = "1", name = "Electronics" } };
                    return new(new ResolveReferenceResult(type, product));
                },
                ["Review"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var review = new { id, body = $"Review {id}", product = new { upc = "100", name = "Product 100" } };
                    return new(new ResolveReferenceResult(type, review));
                },
                ["Category"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var category = new { id, name = $"Category {id}" };
                    return new(new ResolveReferenceResult(type, category));
                }
            }))
            .Add(new ResolversMap
            {
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "username", context => context.ResolveAsPropertyOf<dynamic>(u => u.username) },
                    { "reviews", context => context.ResolveAs(new[] { new { id = "1001", body = "Great product!" } }) }
                },
                ["Product"] = new()
                {
                    { "upc", context => context.ResolveAsPropertyOf<dynamic>(p => p.upc) },
                    { "name", context => context.ResolveAsPropertyOf<dynamic>(p => p.name) },
                    { "category", context => context.ResolveAsPropertyOf<dynamic>(p => p.category) }
                },
                ["Review"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(r => r.id) },
                    { "body", context => context.ResolveAsPropertyOf<dynamic>(r => r.body) },
                    { "product", context => context.ResolveAsPropertyOf<dynamic>(r => r.product) }
                },
                ["Category"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(c => c.id) },
                    { "name", context => context.ResolveAsPropertyOf<dynamic>(c => c.name) }
                }
            });

        return await builder.Build();
    }

    private static async Task<ISchema> CreateCircularReferenceSchema()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") {
                    id: ID!
                    username: String!
                    friends: [User!]!
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
                }
            }))
            .Add(new ResolversMap
            {
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "username", context => context.ResolveAsPropertyOf<dynamic>(u => u.username) },
                    { "friends", context => 
                        {
                            var user = (dynamic)context.ObjectValue;
                            var userId = user.id;
                            // Return different friends based on user ID to avoid infinite loops
                            return context.ResolveAs(new[] { new { id = userId == "1" ? "2" : "1", username = userId == "1" ? "@user2" : "@user1" } });
                        }
                    }
                }
            });

        return await builder.Build();
    }

    private static async Task<ISchema> CreateDeeplyNestedSchema()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type Organization @key(fields: ""id"") {
                    id: ID!
                    name: String!
                    departments: [Department!]!
                }

                type Department @key(fields: ""id"") {
                    id: ID!
                    name: String!
                    teams: [Team!]!
                }

                type Team @key(fields: ""id"") {
                    id: ID!
                    name: String!
                    members: [User!]!
                }

                type User @key(fields: ""id"") {
                    id: ID!
                    username: String!
                    role: String!
                }

                type Query {
                    # Empty query type
                }
            ")
            .AddSubgraph(new(new DictionaryReferenceResolversMap
            {
                ["Organization"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var org = new { id, name = $"Organization {id}" };
                    return new(new ResolveReferenceResult(type, org));
                },
                ["Department"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var dept = new { id, name = $"Department {id}" };
                    return new(new ResolveReferenceResult(type, dept));
                },
                ["Team"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var team = new { id, name = $"Team {id}" };
                    return new(new ResolveReferenceResult(type, team));
                },
                ["User"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var user = new { id, username = $"@user{id}", role = "Developer" };
                    return new(new ResolveReferenceResult(type, user));
                }
            }))
            .Add(new ResolversMap
            {
                ["Organization"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(o => o.id) },
                    { "name", context => context.ResolveAsPropertyOf<dynamic>(o => o.name) },
                    { "departments", context => context.ResolveAs(new[] { new { id = "1", name = "Engineering" }, new { id = "2", name = "Marketing" } }) }
                },
                ["Department"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(d => d.id) },
                    { "name", context => context.ResolveAsPropertyOf<dynamic>(d => d.name) },
                    { "teams", context => context.ResolveAs(new[] { new { id = "1", name = "Backend Team" }, new { id = "2", name = "Frontend Team" } }) }
                },
                ["Team"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(t => t.id) },
                    { "name", context => context.ResolveAsPropertyOf<dynamic>(t => t.name) },
                    { "members", context => context.ResolveAs(new[] { new { id = "1", username = "@dev1", role = "Senior Developer" }, new { id = "2", username = "@dev2", role = "Junior Developer" } }) }
                },
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "username", context => context.ResolveAsPropertyOf<dynamic>(u => u.username) },
                    { "role", context => context.ResolveAsPropertyOf<dynamic>(u => u.role) }
                }
            });

        return await builder.Build();
    }

    private static async Task<ISchema> CreateLargeResultSetSchema()
    {
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                type User @key(fields: ""id"") {
                    id: ID!
                    username: String!
                    posts: [Post!]!
                }

                type Post @key(fields: ""id"") {
                    id: ID!
                    title: String!
                    comments: [Comment!]!
                }

                type Comment @key(fields: ""id"") {
                    id: ID!
                    body: String!
                    author: User!
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
                ["Post"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var post = new { id, title = $"Post {id}" };
                    return new(new ResolveReferenceResult(type, post));
                },
                ["Comment"] = (context, type, representation) =>
                {
                    var id = representation["id"].ToString();
                    var comment = new { id, body = $"Comment {id}", author = new { id = "1", username = "@user1" } };
                    return new(new ResolveReferenceResult(type, comment));
                }
            }))
            .Add(new ResolversMap
            {
                ["User"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(u => u.id) },
                    { "username", context => context.ResolveAsPropertyOf<dynamic>(u => u.username) },
                    { "posts", context => context.ResolveAs(Enumerable.Range(1, 15).Select(i => new { id = i.ToString(), title = $"Post {i}" })) }
                },
                ["Post"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(p => p.id) },
                    { "title", context => context.ResolveAsPropertyOf<dynamic>(p => p.title) },
                    { "comments", context => 
                        {
                            var post = (dynamic)context.ObjectValue;
                            var postId = post.id;
                            return context.ResolveAs(Enumerable.Range(1, 5).Select(i => new { id = $"{postId}-{i}", body = $"Comment {i} on post {postId}" }));
                        }
                    }
                },
                ["Comment"] = new()
                {
                    { "id", context => context.ResolveAsPropertyOf<dynamic>(c => c.id) },
                    { "body", context => context.ResolveAsPropertyOf<dynamic>(c => c.body) },
                    { "author", context => context.ResolveAs(new { id = "1", username = "@commenter" }) }
                }
            });

        return await builder.Build();
    }
}