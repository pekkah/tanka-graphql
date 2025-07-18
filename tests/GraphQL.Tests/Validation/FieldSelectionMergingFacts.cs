using System;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Validation;

using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public class FieldSelectionMergingFacts
{
    private readonly ISchema _schema;
    private readonly ResolversMap _resolvers;

    public FieldSelectionMergingFacts()
    {
        var sdl = @"
            interface Node {
                id: ID!
            }

            type User implements Node {
                id: ID!
                name: String!
                email: String!
                age: Int
            }

            type Product implements Node {
                id: ID!
                name: String!
                price: Float!
                category: String
            }

            union SearchResult = User | Product

            type Query {
                user(id: ID!): User
                product(id: ID!): Product
                node(id: ID!): Node
                search(query: String!): SearchResult
            }
        ";

        _resolvers = new ResolversMap
        {
            {
                "User", new FieldResolversMap
                {
                    { "id", context => context.ResolveAs("user1") },
                    { "name", context => context.ResolveAs("John") },
                    { "email", context => context.ResolveAs("john@example.com") },
                    { "age", context => context.ResolveAs(25) }
                }
            },
            {
                "Product", new FieldResolversMap
                {
                    { "id", context => context.ResolveAs("product1") },
                    { "name", context => context.ResolveAs("Widget") },
                    { "price", context => context.ResolveAs(19.99f) },
                    { "category", context => context.ResolveAs("Electronics") }
                }
            },
            {
                "Query", new FieldResolversMap
                {
                    { "user", context => context.ResolveAs(new { }) },
                    { "product", context => context.ResolveAs(new { }) },
                    { "node", context => context.ResolveAs(new { }) },
                    { "search", context => context.ResolveAs(new { }) }
                }
            }
        };

        _schema = new SchemaBuilder()
            .Add(sdl)
            .Build(_resolvers, _resolvers).Result;
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_SimpleFieldMerging_IsValid()
    {
        /* Given */
        var query = @"
            query {
                user(id: ""1"") {
                    id
                    name
                    id
                }
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_FieldMergingWithAlias_IsValid()
    {
        /* Given */
        var query = @"
            query {
                user(id: ""1"") {
                    id
                    name
                    userId: id
                }
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_ConflictingFieldTypes_IsInvalid()
    {
        /* Given */
        var query = @"
            query {
                user(id: ""1"") {
                    name
                    name: age
                }
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Fields \"name\" conflict"));
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_ConflictingArguments_IsInvalid()
    {
        /* Given */
        var query = @"
            query {
                user(id: ""1"") {
                    id
                }
                user(id: ""2"") {
                    id
                }
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Fields \"user\" conflict"));
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_FragmentFieldMerging_IsValid()
    {
        /* Given */
        var query = @"
            query {
                user(id: ""1"") {
                    id
                    name
                    ...userFields
                }
            }
            
            fragment userFields on User {
                id
                email
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_ConflictingFragmentFields_IsInvalid()
    {
        /* Given */
        var query = @"
            query {
                user(id: ""1"") {
                    name
                    ...userFields
                }
            }
            
            fragment userFields on User {
                name: email
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Fields \"name\" conflict"));
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_InlineFragmentFieldMerging_IsValid()
    {
        /* Given */
        var query = @"
            query {
                node(id: ""1"") {
                    id
                    ... on User {
                        id
                        name
                    }
                }
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_ConflictingInlineFragmentFields_IsInvalid()
    {
        /* Given */
        var query = @"
            query {
                node(id: ""1"") {
                    id
                    ... on User {
                        id: name
                    }
                }
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Fields \"id\" conflict"));
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_UnionFieldMerging_IsValid()
    {
        /* Given */
        var query = @"
            query {
                search(query: ""test"") {
                    ... on User {
                        id
                        name
                    }
                    ... on Product {
                        id
                        name
                    }
                }
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_ConflictingUnionFields_IsInvalid()
    {
        /* Given */
        var query = @"
            query {
                search(query: ""test"") {
                    ... on User {
                        name
                        value: age
                    }
                    ... on Product {
                        name
                        value: price
                    }
                }
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Fields \"value\" conflict"));
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_ComplexNestedFieldMerging_IsValid()
    {
        /* Given */
        var query = @"
            query {
                user(id: ""1"") {
                    id
                    name
                    ...userFragment
                }
            }
            
            fragment userFragment on User {
                id
                email
                ... on User {
                    id
                    name
                }
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_ConflictingNestedFields_IsInvalid()
    {
        /* Given */
        var query = @"
            query {
                user(id: ""1"") {
                    id
                    name
                    ...userFragment
                }
            }
            
            fragment userFragment on User {
                id: email
                name: age
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("conflict"));
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_SameFieldDifferentSubselections_IsValid()
    {
        /* Given */
        var query = @"
            query {
                user(id: ""1"") {
                    id
                    name
                }
                user(id: ""1"") {
                    id
                    email
                }
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_ConflictingFieldsWithSubselections_IsInvalid()
    {
        /* Given */
        var query = @"
            query {
                user(id: ""1"") {
                    id
                    name
                }
                user(id: ""2"") {
                    id
                    name
                }
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Fields \"user\" conflict"));
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_MultipleFragmentsWithMerging_IsValid()
    {
        /* Given */
        var query = @"
            query {
                user(id: ""1"") {
                    ...fragment1
                    ...fragment2
                }
            }
            
            fragment fragment1 on User {
                id
                name
            }
            
            fragment fragment2 on User {
                id
                email
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFieldSelectionMerging_ConflictingFragments_IsInvalid()
    {
        /* Given */
        var query = @"
            query {
                user(id: ""1"") {
                    ...fragment1
                    ...fragment2
                }
            }
            
            fragment fragment1 on User {
                id
                value: name
            }
            
            fragment fragment2 on User {
                id
                value: email
            }";

        /* When */
        var result = await ValidateQuery(query);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Fields \"value\" conflict"));
    }

    private async Task<ValidationResult> ValidateQuery(string query)
    {
        var request = new GraphQLRequest { Query = query };
        var validator = new Validator(_schema);
        return await validator.Validate(request);
    }
}