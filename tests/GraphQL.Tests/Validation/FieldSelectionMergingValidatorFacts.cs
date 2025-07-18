using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public class FieldSelectionMergingValidatorFacts
{
    [Fact]
    public async Task Validate_ShouldAllowIdenticalFields()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    name
                    name
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.DidNotReceive().Error(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldAllowIdenticalFieldsWithIdenticalArguments()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    posts(first: 10) {
                        title
                    }
                    posts(first: 10) {
                        title
                    }
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.DidNotReceive().Error(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldAllowIdenticalFieldsWithIdenticalAliases()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    userName: name
                    userName: name
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.DidNotReceive().Error(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldRejectFieldsWithDifferentArguments()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    posts(first: 10) {
                        title
                    }
                    posts(first: 20) {
                        title
                    }
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.Received().Error(
            ValidationErrorCodes.R532FieldSelectionMerging,
            Arg.Is<string>(s => s.Contains("posts") && s.Contains("differing arguments")),
            Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldRejectDifferentFieldsWithSameAlias()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    info: name
                    info: email
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.Received().Error(
            ValidationErrorCodes.R532FieldSelectionMerging,
            Arg.Is<string>(s => s.Contains("info") && s.Contains("different fields")),
            Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldRejectFieldsWithConflictingReturnTypes()
    {
        // Given
        var schema = await CreateSchemaWithConflictingTypes();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    id
                }
                admin {
                    id
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var querySelectionSet = GetQuerySelectionSet(document);
        validator.Validate(querySelectionSet);
        
        // Then
        context.Received().Error(
            ValidationErrorCodes.R532FieldSelectionMerging,
            Arg.Is<string>(s => s.Contains("conflicting types")),
            Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleComplexFragmentMerging()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    ...UserInfo
                    ...UserInfo
                }
            }
            
            fragment UserInfo on User {
                name
                email
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.DidNotReceive().Error(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleConflictingFragmentFields()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    ...UserInfo
                    ...UserDetails
                }
            }
            
            fragment UserInfo on User {
                posts(first: 10) {
                    title
                }
            }
            
            fragment UserDetails on User {
                posts(first: 20) {
                    title
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.Received().Error(
            ValidationErrorCodes.R532FieldSelectionMerging,
            Arg.Is<string>(s => s.Contains("posts") && s.Contains("differing arguments")),
            Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleInlineFragmentMerging()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    name
                    ... on User {
                        name
                        email
                    }
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.DidNotReceive().Error(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleNestedSelectionConflicts()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    posts {
                        title
                        author {
                            name
                        }
                    }
                    posts {
                        title
                        author {
                            email
                        }
                    }
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.DidNotReceive().Error(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleInterfaceFieldMerging()
    {
        // Given
        var schema = await CreateSchemaWithInterface();
        var document = Parser.ParseExecutableDocument(@"
            query {
                character {
                    name
                    ... on Human {
                        name
                        homeworld
                    }
                    ... on Droid {
                        name
                        function
                    }
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var characterSelectionSet = GetCharacterSelectionSet(document);
        validator.Validate(characterSelectionSet);
        
        // Then
        context.DidNotReceive().Error(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleUnionFieldMerging()
    {
        // Given
        var schema = await CreateSchemaWithUnion();
        var document = Parser.ParseExecutableDocument(@"
            query {
                searchResult {
                    ... on User {
                        name
                        email
                    }
                    ... on Post {
                        title
                        content
                    }
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var searchResultSelectionSet = GetSearchResultSelectionSet(document);
        validator.Validate(searchResultSelectionSet);
        
        // Then
        context.DidNotReceive().Error(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleComplexNestedFragmentConflicts()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    ...UserFragment
                    posts {
                        ...PostFragment
                    }
                }
            }
            
            fragment UserFragment on User {
                name
                posts {
                    title
                    ...PostFragment
                }
            }
            
            fragment PostFragment on Post {
                title
                author {
                    name
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.DidNotReceive().Error(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleVariableArgumentMerging()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query($first: Int!) {
                user {
                    posts(first: $first) {
                        title
                    }
                    posts(first: $first) {
                        content
                    }
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        context.VariableValues.Returns(new Dictionary<string, object?> { { "first", 10 } });
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.DidNotReceive().Error(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleConflictingVariableArguments()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query($first: Int!, $second: Int!) {
                user {
                    posts(first: $first) {
                        title
                    }
                    posts(first: $second) {
                        title
                    }
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        context.VariableValues.Returns(new Dictionary<string, object?> { { "first", 10 }, { "second", 20 } });
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.Received().Error(
            ValidationErrorCodes.R532FieldSelectionMerging,
            Arg.Is<string>(s => s.Contains("posts") && s.Contains("differing arguments")),
            Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleEmptySelectionSet()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.DidNotReceive().Error(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleNullParentType()
    {
        // Given
        var schema = await CreateTestSchema();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    name
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        context.Tracker.ParentType.Returns((TypeDefinition?)null);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var userSelectionSet = GetUserSelectionSet(document);
        validator.Validate(userSelectionSet);
        
        // Then
        context.DidNotReceive().Error(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleListAndNonListTypeConflicts()
    {
        // Given
        var schema = await CreateSchemaWithListConflicts();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    tags
                }
                admin {
                    tags
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var querySelectionSet = GetQuerySelectionSet(document);
        validator.Validate(querySelectionSet);
        
        // Then
        context.Received().Error(
            ValidationErrorCodes.R532FieldSelectionMerging,
            Arg.Is<string>(s => s.Contains("conflicting types")),
            Arg.Any<IEnumerable<INode>>());
    }

    [Fact]
    public async Task Validate_ShouldHandleNonNullAndNullableTypeConflicts()
    {
        // Given
        var schema = await CreateSchemaWithNullabilityConflicts();
        var document = Parser.ParseExecutableDocument(@"
            query {
                user {
                    name
                }
                admin {
                    name
                }
            }
        ");
        
        var context = CreateValidationContext(schema, document);
        var validator = new FieldSelectionMergingValidator(context);
        
        // When
        var querySelectionSet = GetQuerySelectionSet(document);
        validator.Validate(querySelectionSet);
        
        // Then
        context.Received().Error(
            ValidationErrorCodes.R532FieldSelectionMerging,
            Arg.Is<string>(s => s.Contains("conflicting types")),
            Arg.Any<IEnumerable<INode>>());
    }

    // Helper methods
    private static async Task<ISchema> CreateTestSchema()
    {
        return await new SchemaBuilder()
            .Add(@"
                type User {
                    name: String!
                    email: String!
                    posts(first: Int): [Post!]!
                }
                
                type Post {
                    title: String!
                    content: String!
                    author: User!
                }
                
                type Query {
                    user: User
                }
            ")
            .Build(new SchemaBuildOptions());
    }

    private static async Task<ISchema> CreateSchemaWithConflictingTypes()
    {
        return await new SchemaBuilder()
            .Add(@"
                type User {
                    id: String!
                    name: String!
                }
                
                type Admin {
                    id: Int!
                    name: String!
                }
                
                type Query {
                    user: User
                    admin: Admin
                }
            ")
            .Build(new SchemaBuildOptions());
    }

    private static async Task<ISchema> CreateSchemaWithInterface()
    {
        return await new SchemaBuilder()
            .Add(@"
                interface Character {
                    name: String!
                }
                
                type Human implements Character {
                    name: String!
                    homeworld: String!
                }
                
                type Droid implements Character {
                    name: String!
                    function: String!
                }
                
                type Query {
                    character: Character
                }
            ")
            .Build(new SchemaBuildOptions());
    }

    private static async Task<ISchema> CreateSchemaWithUnion()
    {
        return await new SchemaBuilder()
            .Add(@"
                type User {
                    name: String!
                    email: String!
                }
                
                type Post {
                    title: String!
                    content: String!
                }
                
                union SearchResult = User | Post
                
                type Query {
                    searchResult: SearchResult
                }
            ")
            .Build(new SchemaBuildOptions());
    }

    private static async Task<ISchema> CreateSchemaWithListConflicts()
    {
        return await new SchemaBuilder()
            .Add(@"
                type User {
                    tags: [String!]!
                }
                
                type Admin {
                    tags: String!
                }
                
                type Query {
                    user: User
                    admin: Admin
                }
            ")
            .Build(new SchemaBuildOptions());
    }

    private static async Task<ISchema> CreateSchemaWithNullabilityConflicts()
    {
        return await new SchemaBuilder()
            .Add(@"
                type User {
                    name: String!
                }
                
                type Admin {
                    name: String
                }
                
                type Query {
                    user: User
                    admin: Admin
                }
            ")
            .Build(new SchemaBuildOptions());
    }

    private static IRuleVisitorContext CreateValidationContext(ISchema schema, ExecutableDocument document)
    {
        var context = Substitute.For<IRuleVisitorContext>();
        var tracker = Substitute.For<TypeTracker>();
        
        context.Schema.Returns(schema);
        context.Document.Returns(document);
        context.Tracker.Returns(tracker);
        context.VariableValues.Returns(new Dictionary<string, object?>());
        
        tracker.ParentType.Returns(schema.GetRequiredNamedType<ObjectDefinition>("User"));
        
        return context;
    }

    private static SelectionSet GetUserSelectionSet(ExecutableDocument document)
    {
        var query = document.OperationDefinitions.First();
        var userField = query.SelectionSet.OfType<FieldSelection>().First(f => f.Name == "user");
        return userField.SelectionSet;
    }

    private static SelectionSet GetQuerySelectionSet(ExecutableDocument document)
    {
        var query = document.OperationDefinitions.First();
        return query.SelectionSet;
    }

    private static SelectionSet GetCharacterSelectionSet(ExecutableDocument document)
    {
        var query = document.OperationDefinitions.First();
        var characterField = query.SelectionSet.OfType<FieldSelection>().First(f => f.Name == "character");
        return characterField.SelectionSet;
    }

    private static SelectionSet GetSearchResultSelectionSet(ExecutableDocument document)
    {
        var query = document.OperationDefinitions.First();
        var searchResultField = query.SelectionSet.OfType<FieldSelection>().First(f => f.Name == "searchResult");
        return searchResultField.SelectionSet;
    }
}