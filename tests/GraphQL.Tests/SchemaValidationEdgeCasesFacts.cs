using System;
using System.Threading.Tasks;

using Tanka.GraphQL.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Tests;

public class SchemaValidationEdgeCasesFacts
{
    [Fact]
    public async Task Schema_WithCircularTypeReferences_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type TypeA {
                typeB: [TypeB]
            }
            
            type TypeB {
                typeA: TypeA
            }
            
            type Query {
                test: String
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify circular references work
        Assert.NotNull(schema.GetNamedType("TypeA"));
        Assert.NotNull(schema.GetNamedType("TypeB"));
    }

    [Fact]
    public async Task Schema_WithValidInterfaceImplementation_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            interface Node {
                id: ID!
                name: String!
            }
            
            type User implements Node {
                id: ID!
                name: String!
                email: String
            }
            
            type Query {
                node(id: ID!): Node
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify interface implementation
        Assert.NotNull(schema.GetNamedType("Node"));
        Assert.NotNull(schema.GetNamedType("User"));
    }

    [Fact]
    public async Task Schema_WithValidEnum_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            enum Status {
                ACTIVE
                INACTIVE
                PENDING
            }
            
            type User {
                id: ID!
                status: Status!
            }
            
            type Query {
                users: [User!]!
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify enum exists
        Assert.NotNull(schema.GetNamedType("Status"));
    }

    [Fact]
    public async Task Schema_WithValidInputObject_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            input UserInput {
                name: String!
                email: String!
                age: Int
            }
            
            type User {
                id: ID!
                name: String!
                email: String!
                age: Int
            }
            
            type Query {
                createUser(input: UserInput!): User
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify input type exists
        Assert.NotNull(schema.GetNamedType("UserInput"));
    }

    [Fact]
    public async Task Schema_WithValidUnion_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            union SearchResult = User | Post
            
            type User {
                id: ID!
                name: String!
            }
            
            type Post {
                id: ID!
                title: String!
            }
            
            type Query {
                search: SearchResult
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify union exists
        Assert.NotNull(schema.GetNamedType("SearchResult"));
    }

    [Fact]
    public async Task Schema_WithDeepNesting_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Level1 {
                level2: Level2
            }
            
            type Level2 {
                level3: Level3
            }
            
            type Level3 {
                level4: Level4
            }
            
            type Level4 {
                level5: Level5
            }
            
            type Level5 {
                value: String
            }
            
            type Query {
                deep: Level1
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify all nested types exist
        Assert.NotNull(schema.GetNamedType("Level1"));
        Assert.NotNull(schema.GetNamedType("Level2"));
        Assert.NotNull(schema.GetNamedType("Level3"));
        Assert.NotNull(schema.GetNamedType("Level4"));
        Assert.NotNull(schema.GetNamedType("Level5"));
    }

    [Fact]
    public async Task Schema_WithSelfReferencingType_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type TreeNode {
                id: ID!
                value: String!
                parent: TreeNode
                children: [TreeNode!]!
            }
            
            type Query {
                root: TreeNode
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify self-referencing type exists
        Assert.NotNull(schema.GetNamedType("TreeNode"));
    }

    [Fact]
    public async Task Schema_WithValidDirectives_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            directive @auth(requires: String!) on FIELD_DEFINITION
            
            type User {
                id: ID!
                name: String!
                email: String! @auth(requires: ""user"")
                admin: Boolean @auth(requires: ""admin"")
            }
            
            type Query {
                me: User @auth(requires: ""user"")
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify custom directive exists  
        Assert.NotNull(schema.GetDirectiveType("auth"));
    }

    [Fact]
    public async Task Schema_WithComplexScalarTypes_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            scalar DateTime
            scalar JSON
            scalar URL
            
            type Event {
                id: ID!
                timestamp: DateTime!
                data: JSON
                source: URL
            }
            
            type Query {
                events: [Event!]!
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify custom scalars exist
        Assert.NotNull(schema.GetNamedType("DateTime"));
        Assert.NotNull(schema.GetNamedType("JSON"));
        Assert.NotNull(schema.GetNamedType("URL"));
    }

    [Fact]
    public async Task Schema_WithListAndNonNullCombinations_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type User {
                id: ID!
                tags: [String!]!        # Non-null list of non-null strings
                friends: [User!]        # Nullable list of non-null users
                groups: [String]!       # Non-null list of nullable strings
                settings: [String]      # Nullable list of nullable strings
            }
            
            type Query {
                users: [User!]!
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify User type exists
        Assert.NotNull(schema.GetNamedType("User"));
    }

    [Fact]
    public async Task Schema_WithArgumentDefaults_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                users(
                    limit: Int = 10
                    offset: Int = 0
                    active: Boolean = true
                    sort: String = ""name""
                ): [User!]!
                
                search(
                    query: String!
                    limit: Int = 20
                ): [User!]!
            }
            
            type User {
                id: ID!
                name: String!
                active: Boolean!
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task Schema_WithExtremeLengthTypeNames_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        var longTypeName = new string('A', 100); // Long but reasonable type name

        builder.Add($@"
            type {longTypeName} {{
                id: ID!
                value: String
            }}
            
            type Query {{
                get{longTypeName}: {longTypeName}
            }}
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify long type name exists
        Assert.NotNull(schema.GetNamedType(longTypeName));
    }

    [Fact]
    public async Task Schema_WithInterfaceCovariance_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            interface Node {
                id: ID
                process(input: String): String
            }
            
            type User implements Node {
                id: ID!               # Covariant - more specific return type
                name: String!
                process(input: String!): String!  # Covariant - more specific argument and return types
            }
            
            type Query {
                node(id: ID!): Node
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify interface covariance works
        Assert.NotNull(schema.GetNamedType("Node"));
        Assert.NotNull(schema.GetNamedType("User"));
    }

    [Fact]
    public async Task Schema_WithEmptyObjectTypes_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type EmptyType {
                # This type intentionally has no fields for testing edge cases
            }
            
            type Query {
                empty: EmptyType
                test: String
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify empty type exists (though it may not be valid GraphQL)
        Assert.NotNull(schema.GetNamedType("EmptyType"));
    }

    [Fact]
    public async Task Schema_WithMutationAndSubscription_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                hello: String
            }
            
            type Mutation {
                updateUser(input: UserInput!): User
            }
            
            type Subscription {
                userUpdated: User
            }
            
            input UserInput {
                name: String!
            }
            
            type User {
                id: ID!
                name: String!
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify all operation types exist
        Assert.NotNull(schema.GetNamedType("Query"));
        Assert.NotNull(schema.GetNamedType("Mutation"));
        Assert.NotNull(schema.GetNamedType("Subscription"));
    }

    [Fact]
    public async Task Schema_WithComplexFieldArguments_ShouldBuildSuccessfully()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                search(
                    text: String!
                    filters: [FilterInput!]
                    pagination: PaginationInput = {limit: 10, offset: 0}
                    sortBy: [SortInput!] = [{field: ""relevance"", direction: DESC}]
                ): SearchResult
            }
            
            input FilterInput {
                field: String!
                operator: FilterOperator!
                value: String!
            }
            
            input PaginationInput {
                limit: Int! = 10
                offset: Int! = 0
            }
            
            input SortInput {
                field: String!
                direction: SortDirection! = ASC
            }
            
            enum FilterOperator {
                EQUALS
                CONTAINS
                STARTS_WITH
                ENDS_WITH
            }
            
            enum SortDirection {
                ASC
                DESC
            }
            
            type SearchResult {
                items: [SearchItem!]!
                totalCount: Int!
            }
            
            type SearchItem {
                id: ID!
                title: String!
                relevance: Float!
            }
        ");

        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);

        // Verify complex argument types exist
        Assert.NotNull(schema.GetNamedType("FilterInput"));
        Assert.NotNull(schema.GetNamedType("PaginationInput"));
        Assert.NotNull(schema.GetNamedType("SortInput"));
        Assert.NotNull(schema.GetNamedType("FilterOperator"));
        Assert.NotNull(schema.GetNamedType("SortDirection"));
    }
}