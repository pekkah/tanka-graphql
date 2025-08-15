using System.Threading.Tasks;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;

using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

/// <summary>
/// Tests for Apollo Federation v2.3 with @link directive support
/// </summary>
public class FederationV2Facts
{
    [Fact]
    public async Task Should_support_basic_federation_subgraph()
    {
        /* Given */
        var schema = @"
schema @link(url: ""https://specs.apollo.dev/federation/v2.3"", import: [""@key"", ""FieldSet"", ""_Any"", ""_Service""]) {
  query: Query
}

type Product @key(fields: ""id"") {
    id: ID!
    name: String
}

type Query {
    product(id: ID!): Product
}";

        var builder = new ExecutableSchemaBuilder()
            .Add(schema);

        /* When */
        var subgraphOptions = new SubgraphOptions(new DictionaryReferenceResolversMap());
        var result = await builder.Build(options =>
        {
            options.UseFederation(subgraphOptions);
        });

        /* Then */
        Assert.NotNull(result);

        // Verify Federation types are available through @link
        var fieldSetType = result.GetNamedType("FieldSet");
        var anyType = result.GetNamedType("_Any");
        var serviceType = result.GetNamedType("_Service");

        Assert.NotNull(fieldSetType);
        Assert.NotNull(anyType);
        Assert.NotNull(serviceType);
    }

    [Fact]
    public async Task Should_include_federation_directives()
    {
        /* Given */
        var schema = @"
schema @link(url: ""https://specs.apollo.dev/federation/v2.3"", import: [""@key"", ""@shareable"", ""@inaccessible"", ""@override"", ""FieldSet"", ""_Any"", ""_Service""]) {
  query: Query
}

type Product @key(fields: ""id"") @shareable {
    id: ID!
    name: String @inaccessible
}

type Query {
    product(id: ID!): Product
}";

        var builder = new ExecutableSchemaBuilder()
            .Add(schema);

        /* When */
        var subgraphOptions = new SubgraphOptions(new DictionaryReferenceResolversMap());
        var result = await builder.Build(options =>
        {
            options.UseFederation(subgraphOptions);
        });

        /* Then */
        Assert.NotNull(result);

        // Verify Federation v2 directives are available
        var shareableDirective = result.GetDirectiveType("shareable");
        var inaccessibleDirective = result.GetDirectiveType("inaccessible");
        var overrideDirective = result.GetDirectiveType("override");
        var linkDirective = result.GetDirectiveType("link");

        Assert.NotNull(shareableDirective);
        Assert.NotNull(inaccessibleDirective);
        Assert.NotNull(overrideDirective);
        Assert.NotNull(linkDirective);
    }

    [Fact]
    public async Task Should_create_entity_union_for_key_types()
    {
        /* Given */
        var schema = @"
schema @link(url: ""https://specs.apollo.dev/federation/v2.3"", import: [""@key"", ""_Entity"", ""FieldSet"", ""_Any"", ""_Service""]) {
  query: Query
}

type Product @key(fields: ""id"") {
    id: ID!
    name: String
}

type User @key(fields: ""email"") {
    email: String!
    name: String
}

type Order {
    id: ID!
    total: Float
}

type Query {
    product(id: ID!): Product
    user(email: String!): User  
    order(id: ID!): Order
}";

        var builder = new ExecutableSchemaBuilder()
            .Add(schema);

        /* When */
        var subgraphOptions = new SubgraphOptions(new DictionaryReferenceResolversMap());
        var result = await builder.Build(options =>
        {
            options.UseFederation(subgraphOptions);
        });

        /* Then */
        Assert.NotNull(result);

        var entityUnion = result.GetNamedType("_Entity") as UnionDefinition;
        Assert.NotNull(entityUnion);

        var possibleTypes = result.GetPossibleTypes(entityUnion);

        // Should include Product and User (have @key) but not Order (no @key)
        Assert.Contains(possibleTypes, t => t.Name == "Product");
        Assert.Contains(possibleTypes, t => t.Name == "User");
        Assert.DoesNotContain(possibleTypes, t => t.Name == "Order");
    }

    [Fact]
    public async Task Should_support_custom_federation_spec_url()
    {
        /* Given */
        var schema = @"
schema @link(url: ""https://specs.apollo.dev/federation/v2.0"", import: [""@key"", ""FieldSet"", ""_Any"", ""_Service""]) {
  query: Query
}

type Product @key(fields: ""id"") {
    id: ID!
    name: String
}

type Query {
    product(id: ID!): Product
}";

        var options = new SubgraphOptions(new DictionaryReferenceResolversMap())
        {
            FederationSpecUrl = "https://specs.apollo.dev/federation/v2.0"
        };

        var builder = new ExecutableSchemaBuilder()
            .Add(schema);

        /* When */
        var result = await builder.Build(buildOptions =>
        {
            buildOptions.UseFederation(options);
        });

        /* Then */
        Assert.NotNull(result);

        // Should still work with different spec version
        var fieldSetType = result.GetNamedType("FieldSet");
        Assert.NotNull(fieldSetType);
    }

    [Fact]
    public async Task Should_support_custom_import_list()
    {
        /* Given */
        var schema = @"
schema @link(url: ""https://specs.apollo.dev/federation/v2.3"", import: [""@key"", ""@external"", ""FieldSet"", ""_Any"", ""_Service""]) {
  query: Query
}

type Product @key(fields: ""id"") {
    id: ID!
    name: String
}

type Query {
    product(id: ID!): Product
}";

        var options = new SubgraphOptions(new DictionaryReferenceResolversMap())
        {
            ImportList = new[] { "@key", "@external", "FieldSet", "_Any", "_Service" }
        };

        var builder = new ExecutableSchemaBuilder()
            .Add(schema);

        /* When */
        var result = await builder.Build(buildOptions =>
        {
            buildOptions.UseFederation(options);
        });

        /* Then */
        Assert.NotNull(result);

        // Should have imported types
        var fieldSetType = result.GetNamedType("FieldSet");
        var keyDirective = result.GetDirectiveType("key");
        Assert.NotNull(fieldSetType);
        Assert.NotNull(keyDirective);

        // Should not have non-imported directives like @shareable
        var shareableDirective = result.GetDirectiveType("shareable");
        Assert.Null(shareableDirective);
    }
}