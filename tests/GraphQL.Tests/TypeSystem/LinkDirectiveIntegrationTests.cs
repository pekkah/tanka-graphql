using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem;

public class LinkDirectiveIntegrationTests
{
    [Fact]
    public async Task SchemaBuilder_WithLinkDirective_ShouldLoadLinkedSchema()
    {
        // Given
        var mainSchema = @"
extend schema @link(url: ""./types.graphql"")

type Query {
    user: User
}";

        var linkedSchema = @"
type User {
    id: ID!
    name: String!
}";

        var mockLoader = Substitute.For<ISchemaLoader>();
        mockLoader.LoadSchemaAsync("./types.graphql", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TypeSystemDocument?>(Parser.Create(linkedSchema).ParseTypeSystemDocument()));

        var builder = new SchemaBuilder();
        builder.Add(mainSchema);

        // When
        var schema = await builder.Build(options =>
        {
            options.SchemaLoader = mockLoader;
            options.Resolvers = ResolversMap.None;
        });

        // Then
        Assert.NotNull(schema);
        var userType = schema.GetNamedType("User") as ObjectDefinition;
        Assert.NotNull(userType);
        Assert.Equal("User", userType.Name.Value);
        Assert.Equal(2, userType.Fields?.Count);
    }

    [Fact]
    public async Task SchemaBuilder_WithLinkDirective_AndImportFilter_ShouldOnlyImportSpecifiedTypes()
    {
        // Given
        var mainSchema = @"
extend schema @link(url: ""./types.graphql"", import: [""User""])

type Query {
    user: User
}";

        var linkedSchema = @"
type User {
    id: ID!
    name: String!
}

type Post {
    id: ID!
    title: String!
}";

        var mockLoader = Substitute.For<ISchemaLoader>();
        mockLoader.LoadSchemaAsync("./types.graphql", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TypeSystemDocument?>(Parser.Create(linkedSchema).ParseTypeSystemDocument()));

        var builder = new SchemaBuilder();
        builder.Add(mainSchema);

        // When
        var schema = await builder.Build(options =>
        {
            options.SchemaLoader = mockLoader;
            options.Resolvers = ResolversMap.None;
        });

        // Then
        Assert.NotNull(schema);
        var userType = schema.GetNamedType("User") as ObjectDefinition;
        Assert.NotNull(userType);
        var postType = schema.GetNamedType("Post") as ObjectDefinition;
        Assert.Null(postType); // Post should not be imported
    }

    [Fact]
    public async Task SchemaBuilder_WithMultipleLinkDirectives_ShouldLoadAllLinkedSchemas()
    {
        // Given
        var mainSchema = @"
extend schema 
    @link(url: ""./users.graphql"", import: [""User""])
    @link(url: ""./posts.graphql"", import: [""Post""])

type Query {
    user: User
    post: Post
}";

        var usersSchema = @"
type User {
    id: ID!
    name: String!
}";

        var postsSchema = @"
type Post {
    id: ID!
    title: String!
}";

        var mockLoader = Substitute.For<ISchemaLoader>();
        mockLoader.LoadSchemaAsync("./users.graphql", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TypeSystemDocument?>(Parser.Create(usersSchema).ParseTypeSystemDocument()));
        mockLoader.LoadSchemaAsync("./posts.graphql", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TypeSystemDocument?>(Parser.Create(postsSchema).ParseTypeSystemDocument()));

        var builder = new SchemaBuilder();
        builder.Add(mainSchema);

        // When
        var schema = await builder.Build(options =>
        {
            options.SchemaLoader = mockLoader;
            options.Resolvers = ResolversMap.None;
        });

        // Then
        Assert.NotNull(schema);
        var userType = schema.GetNamedType("User") as ObjectDefinition;
        Assert.NotNull(userType);
        var postType = schema.GetNamedType("Post") as ObjectDefinition;
        Assert.NotNull(postType);
    }

    [Fact]
    public async Task SchemaBuilder_WithNestedLinkDirectives_ShouldResolveDepthFirst()
    {
        // Given
        var mainSchema = @"
extend schema @link(url: ""./level1.graphql"")

type Query {
    user: User
}";

        var level1Schema = @"
extend schema @link(url: ""./level2.graphql"")

type User {
    id: ID!
    address: Address
}";

        var level2Schema = @"
type Address {
    street: String!
    city: String!
}";

        var mockLoader = Substitute.For<ISchemaLoader>();
        mockLoader.LoadSchemaAsync("./level1.graphql", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TypeSystemDocument?>(Parser.Create(level1Schema).ParseTypeSystemDocument()));
        mockLoader.LoadSchemaAsync("./level2.graphql", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TypeSystemDocument?>(Parser.Create(level2Schema).ParseTypeSystemDocument()));

        var builder = new SchemaBuilder();
        builder.Add(mainSchema);

        // When
        var schema = await builder.Build(options =>
        {
            options.SchemaLoader = mockLoader;
            options.Resolvers = ResolversMap.None;
        });

        // Then
        Assert.NotNull(schema);
        var userType = schema.GetNamedType("User") as ObjectDefinition;
        Assert.NotNull(userType);
        var addressType = schema.GetNamedType("Address") as ObjectDefinition;
        Assert.NotNull(addressType);
    }

    [Fact]
    public async Task SchemaBuilder_WithCircularLinkReferences_ShouldNotInfiniteLoop()
    {
        // Given
        var mainSchema = @"
extend schema @link(url: ""./schema1.graphql"")

type Query {
    field: String
}";

        var schema1 = @"
extend schema @link(url: ""./schema2.graphql"")

type Type1 {
    field: String
}";

        var schema2 = @"
extend schema @link(url: ""./schema1.graphql"")

type Type2 {
    field: String
}";

        var mockLoader = Substitute.For<ISchemaLoader>();
        mockLoader.LoadSchemaAsync("./schema1.graphql", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TypeSystemDocument?>(Parser.Create(schema1).ParseTypeSystemDocument()));
        mockLoader.LoadSchemaAsync("./schema2.graphql", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TypeSystemDocument?>(Parser.Create(schema2).ParseTypeSystemDocument()));

        var builder = new SchemaBuilder();
        builder.Add(mainSchema);

        // When
        var schema = await builder.Build(options =>
        {
            options.SchemaLoader = mockLoader;
            options.Resolvers = ResolversMap.None;
        });

        // Then
        Assert.NotNull(schema);
        Assert.NotNull(schema.GetNamedType("Type1") as ObjectDefinition);
        Assert.NotNull(schema.GetNamedType("Type2") as ObjectDefinition);

        // Verify the loader was called only once for each URL (no infinite loop)
        await mockLoader.Received(1).LoadSchemaAsync("./schema1.graphql", Arg.Any<CancellationToken>());
        await mockLoader.Received(1).LoadSchemaAsync("./schema2.graphql", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SchemaBuilder_WithMaxDepthExceeded_ShouldThrowException()
    {
        // Given
        var schemas = new Dictionary<string, string>();
        for (int i = 0; i < 15; i++)
        {
            var nextUrl = i < 14 ? $"@link(url: \"./schema{i + 1}.graphql\")" : "";
            schemas[$"./schema{i}.graphql"] = $@"
extend schema {nextUrl}

type Type{i} {{
    field: String
}}";
        }

        var mockLoader = Substitute.For<ISchemaLoader>();
        foreach (var (url, schemaContent) in schemas)
        {
            mockLoader.LoadSchemaAsync(url, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<TypeSystemDocument?>(Parser.Create(schemaContent).ParseTypeSystemDocument()));
        }

        var builder = new SchemaBuilder();
        builder.Add(schemas["./schema0.graphql"]);

        // When/Then
        await Assert.ThrowsAsync<System.InvalidOperationException>(async () =>
        {
            await builder.Build(options =>
            {
                options.SchemaLoader = mockLoader;
                options.MaxLinkDepth = 10; // Set a lower max depth
                options.Resolvers = ResolversMap.None;
            });
        });
    }

    [Fact]
    public async Task SchemaBuilder_WithDirectiveImport_ShouldImportDirectives()
    {
        // Given
        var mainSchema = @"
extend schema @link(url: ""./directives.graphql"", import: [""@auth""])

type Query @auth {
    user: User
}

type User {
    id: ID!
}";

        var directivesSchema = @"
directive @auth on OBJECT | FIELD_DEFINITION

directive @deprecated(reason: String) on FIELD_DEFINITION";

        var mockLoader = Substitute.For<ISchemaLoader>();
        mockLoader.LoadSchemaAsync("./directives.graphql", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TypeSystemDocument?>(Parser.Create(directivesSchema).ParseTypeSystemDocument()));

        var builder = new SchemaBuilder();
        builder.Add(mainSchema);

        // When
        var schema = await builder.Build(options =>
        {
            options.SchemaLoader = mockLoader;
            options.Resolvers = ResolversMap.None;
            options.IncludeBuiltInTypes = false; // Don't include built-in @deprecated
        });

        // Then
        Assert.NotNull(schema);
        var authDirective = schema.GetDirectiveType("auth");
        Assert.NotNull(authDirective);
        var deprecatedDirective = schema.GetDirectiveType("deprecated");
        Assert.Null(deprecatedDirective); // @deprecated should not be imported
    }

    [Fact]
    public async Task SchemaBuilder_WithNoSchemaLoader_AndLinkDirective_ShouldNotFail()
    {
        // Given
        var mainSchema = @"
extend schema @link(url: ""./types.graphql"")

type Query {
    field: String
}";

        var builder = new SchemaBuilder();
        builder.Add(mainSchema);

        // When
        var schema = await builder.Build(options =>
        {
            // SchemaLoader is null, so ProcessLinkDirectives will be false
            options.Resolvers = ResolversMap.None;
        });

        // Then
        Assert.NotNull(schema);
        Assert.NotNull(schema.GetNamedType("Query") as ObjectDefinition);
        // The @link directive is ignored when no SchemaLoader is provided
    }
}