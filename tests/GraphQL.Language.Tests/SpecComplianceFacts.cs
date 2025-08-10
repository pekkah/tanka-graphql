using System;
using System.Linq;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;

using Xunit;

namespace Tanka.GraphQL.Language.Tests;

/// <summary>
/// Tests to verify that the Tanka GraphQL Language parser produces results
/// that comply with the GraphQL specification examples and requirements.
/// 
/// These tests validate parsing behavior against the current GraphQL specification draft.
/// </summary>
public class SpecComplianceFacts
{
    #region Query Parsing Compliance

    [Fact]
    public void Parse_BasicQuery_ShouldMatchSpecification()
    {
        // Given: Basic query from GraphQL specification
        var query = "{ user { name } }";

        // When: Parse the query
        var document = Parser.Create(query).ParseExecutableDocument();

        // Then: Verify AST structure matches specification
        Assert.NotNull(document);
        Assert.Single(document.OperationDefinitions);

        var operation = document.OperationDefinitions.First();
        Assert.Equal(OperationType.Query, operation.Operation);
        Assert.Null(operation.Name); // Anonymous query

        Assert.Single(operation.SelectionSet);
        var userField = (FieldSelection)operation.SelectionSet.First();
        Assert.Equal("user", userField.Name);

        Assert.Single(userField.SelectionSet);
        var nameField = (FieldSelection)userField.SelectionSet.First();
        Assert.Equal("name", nameField.Name);
    }

    [Fact]
    public void Parse_QueryWithVariables_ShouldMatchSpecification()
    {
        // Given: Query with variables from GraphQL specification
        var query = @"query GetUser($id: ID!, $withFriends: Boolean = false) {
            user(id: $id) {
                name
                friends @include(if: $withFriends) {
                    name
                }
            }
        }";

        // When: Parse the query
        var document = Parser.Create(query).ParseExecutableDocument();

        // Then: Verify operation structure
        var operation = document.OperationDefinitions.First();
        Assert.Equal("GetUser", operation.Name);
        Assert.Equal(OperationType.Query, operation.Operation);

        // Verify variable definitions
        Assert.Equal(2, operation.VariableDefinitions.Count);

        var idVar = operation.VariableDefinitions.First();
        Assert.Equal("id", idVar.Variable.Name);
        Assert.True(idVar.Type is NonNullType);
        Assert.Null(idVar.DefaultValue);

        var withFriendsVar = operation.VariableDefinitions.Skip(1).First();
        Assert.Equal("withFriends", withFriendsVar.Variable.Name);
        Assert.NotNull(withFriendsVar.DefaultValue);
        Assert.False(((BooleanValue)withFriendsVar.DefaultValue.Value).Value);
    }

    [Fact]
    public void Parse_QueryWithFragments_ShouldMatchSpecification()
    {
        // Given: Query with fragments from GraphQL specification
        var query = @"
            query GetUsers {
                users {
                    ...UserInfo
                    friends {
                        ...UserInfo
                    }
                }
            }
            
            fragment UserInfo on User {
                id
                name
                profilePic(size: 50)
            }";

        // When: Parse the query
        var document = Parser.Create(query).ParseExecutableDocument();

        // Then: Verify fragment definition
        Assert.Single(document.FragmentDefinitions);
        var fragment = document.FragmentDefinitions.First();
        Assert.Equal("UserInfo", fragment.FragmentName);
        Assert.Equal("User", fragment.TypeCondition.Name);
        Assert.Equal(3, fragment.SelectionSet.Count);

        // Verify fragment spread in query
        var operation = document.OperationDefinitions.First();
        var usersField = (FieldSelection)operation.SelectionSet.First();
        var fragmentSpread = usersField.SelectionSet.OfType<FragmentSpread>().First();
        Assert.Equal("UserInfo", fragmentSpread.FragmentName);
    }

    #endregion

    #region Mutation Parsing Compliance

    [Fact]
    public void Parse_BasicMutation_ShouldMatchSpecification()
    {
        // Given: Basic mutation from GraphQL specification
        var mutation = @"mutation CreateUser($input: CreateUserInput!) {
            createUser(input: $input) {
                id
                name
                email
            }
        }";

        // When: Parse the mutation
        var document = Parser.Create(mutation).ParseExecutableDocument();

        // Then: Verify mutation structure
        var operation = document.OperationDefinitions.First();
        Assert.Equal(OperationType.Mutation, operation.Operation);
        Assert.Equal("CreateUser", operation.Name);

        // Verify mutation field
        var createUserField = (FieldSelection)operation.SelectionSet.First();
        Assert.Equal("createUser", createUserField.Name);
        Assert.Single(createUserField.Arguments);

        var inputArg = createUserField.Arguments.First();
        Assert.Equal("input", inputArg.Name);
        Assert.IsType<Variable>(inputArg.Value);
    }

    #endregion

    #region Subscription Parsing Compliance

    [Fact]
    public void Parse_BasicSubscription_ShouldMatchSpecification()
    {
        // Given: Basic subscription from GraphQL specification
        var subscription = @"subscription OnUserUpdate($id: ID!) {
            userUpdated(id: $id) {
                id
                name
                lastModified
            }
        }";

        // When: Parse the subscription
        var document = Parser.Create(subscription).ParseExecutableDocument();

        // Then: Verify subscription structure
        var operation = document.OperationDefinitions.First();
        Assert.Equal(OperationType.Subscription, operation.Operation);
        Assert.Equal("OnUserUpdate", operation.Name);

        var subscriptionField = (FieldSelection)operation.SelectionSet.First();
        Assert.Equal("userUpdated", subscriptionField.Name);
    }

    #endregion

    #region Directive Parsing Compliance

    [Fact]
    public void Parse_SkipDirective_ShouldMatchSpecification()
    {
        // Given: Query with @skip directive from GraphQL specification
        var query = @"query GetUser($withEmail: Boolean!) {
            user {
                name
                email @skip(if: $withEmail)
            }
        }";

        // When: Parse the query
        var document = Parser.Create(query).ParseExecutableDocument();

        // Then: Verify @skip directive
        var operation = document.OperationDefinitions.First();
        var userField = (FieldSelection)operation.SelectionSet.First();
        var emailField = (FieldSelection)userField.SelectionSet.Skip(1).First();

        Assert.Single(emailField.Directives);
        var skipDirective = emailField.Directives.First();
        Assert.Equal("skip", skipDirective.Name);
        Assert.Single(skipDirective.Arguments);

        var ifArg = skipDirective.Arguments.First();
        Assert.Equal("if", ifArg.Name);
        Assert.IsType<Variable>(ifArg.Value);
    }

    [Fact]
    public void Parse_IncludeDirective_ShouldMatchSpecification()
    {
        // Given: Query with @include directive from GraphQL specification
        var query = @"query GetUser($includeFriends: Boolean = true) {
            user {
                name
                friends @include(if: $includeFriends) {
                    name
                }
            }
        }";

        // When: Parse the query
        var document = Parser.Create(query).ParseExecutableDocument();

        // Then: Verify @include directive
        var operation = document.OperationDefinitions.First();
        var userField = (FieldSelection)operation.SelectionSet.First();
        var friendsField = (FieldSelection)userField.SelectionSet.Skip(1).First();

        Assert.Single(friendsField.Directives);
        var includeDirective = friendsField.Directives.First();
        Assert.Equal("include", includeDirective.Name);
        Assert.Single(includeDirective.Arguments);
    }

    #endregion

    #region Inline Fragment Parsing Compliance

    [Fact]
    public void Parse_InlineFragment_ShouldMatchSpecification()
    {
        // Given: Query with inline fragments from GraphQL specification
        var query = @"query GetCharacter {
            character {
                name
                ... on Human {
                    height
                }
                ... on Droid {
                    primaryFunction
                }
            }
        }";

        // When: Parse the query
        var document = Parser.Create(query).ParseExecutableDocument();

        // Then: Verify inline fragments
        var operation = document.OperationDefinitions.First();
        var characterField = (FieldSelection)operation.SelectionSet.First();

        var selections = characterField.SelectionSet.ToList();
        Assert.Equal(3, selections.Count); // name + 2 inline fragments

        var humanFragment = (InlineFragment)selections[1];
        Assert.Equal("Human", humanFragment.TypeCondition.Name);
        Assert.Single(humanFragment.SelectionSet);

        var droidFragment = (InlineFragment)selections[2];
        Assert.Equal("Droid", droidFragment.TypeCondition.Name);
        Assert.Single(droidFragment.SelectionSet);
    }

    #endregion

    #region Variable Type Parsing Compliance

    [Fact]
    public void Parse_AllVariableTypes_ShouldMatchSpecification()
    {
        // Given: Query with various variable types from GraphQL specification
        var query = @"query ComplexQuery(
            $id: ID!
            $name: String
            $age: Int!
            $score: Float
            $active: Boolean!
            $tags: [String!]!
            $filters: SearchFilters
        ) {
            search(id: $id, name: $name, age: $age) {
                id
            }
        }";

        // When: Parse the query
        var document = Parser.Create(query).ParseExecutableDocument();

        // Then: Verify all variable types
        var operation = document.OperationDefinitions.First();
        var variables = operation.VariableDefinitions.ToList();

        Assert.Equal(7, variables.Count);

        // ID! (NonNull NamedType)
        var idVar = variables[0];
        Assert.Equal("id", idVar.Variable.Name);
        Assert.IsType<NonNullType>(idVar.Type);
        var idInnerType = ((NonNullType)idVar.Type).OfType;
        Assert.Equal("ID", ((NamedType)idInnerType).Name);

        // String (NamedType)
        var nameVar = variables[1];
        Assert.Equal("name", nameVar.Variable.Name);
        Assert.IsType<NamedType>(nameVar.Type);
        Assert.Equal("String", ((NamedType)nameVar.Type).Name);

        // [String!]! (NonNull ListType of NonNull String)
        var tagsVar = variables[5];
        Assert.Equal("tags", tagsVar.Variable.Name);
        Assert.IsType<NonNullType>(tagsVar.Type);
        var tagsInnerType = ((NonNullType)tagsVar.Type).OfType;
        Assert.IsType<ListType>(tagsInnerType);
        var tagsListItemType = ((ListType)tagsInnerType).OfType;
        Assert.IsType<NonNullType>(tagsListItemType);
    }

    #endregion

    #region Input Object Parsing Compliance

    [Fact]
    public void Parse_InputObjectValues_ShouldMatchSpecification()
    {
        // Given: Query with input object from GraphQL specification
        var query = @"mutation CreateUser {
            createUser(input: {
                name: ""John Doe""
                email: ""john@example.com""
                age: 30
                active: true
                tags: [""developer"", ""typescript""]
                profile: {
                    bio: ""Software developer""
                    website: ""https://johndoe.dev""
                }
            }) {
                id
                name
            }
        }";

        // When: Parse the query
        var document = Parser.Create(query).ParseExecutableDocument();

        // Then: Verify input object structure
        var operation = document.OperationDefinitions.First();
        var createUserField = (FieldSelection)operation.SelectionSet.First();
        var inputArg = createUserField.Arguments.First();

        Assert.Equal("input", inputArg.Name);
        Assert.IsType<ObjectValue>(inputArg.Value);

        var inputObj = (ObjectValue)inputArg.Value;
        Assert.Equal(6, inputObj.Count);

        // Verify nested object
        var profileField = inputObj.First(f => f.Name == "profile");
        Assert.IsType<ObjectValue>(profileField.Value);
        var profileObj = (ObjectValue)profileField.Value;
        Assert.Equal(2, profileObj.Count);
    }

    #endregion

    #region Error Case Parsing (Should Throw)

    [Fact]
    public void Parse_SyntaxError_ShouldThrowWithLocation()
    {
        // Given: Invalid GraphQL syntax
        var invalidQuery = "query { user { name }"; // Missing closing brace

        // When/Then: Should throw parse exception with location
        var exception = Assert.Throws<ParseException>(() =>
            Parser.Create(invalidQuery).ParseExecutableDocument());

        Assert.Contains("Expected", exception.Message);
    }

    [Fact]
    public void Parse_UnknownCharacter_ShouldThrowWithLocation()
    {
        // Given: Invalid character in GraphQL
        var invalidQuery = "query { user { name @ } }"; // Invalid @ usage

        // When/Then: Should throw parse exception
        var exception = Assert.Throws<ParseException>(() =>
            Parser.Create(invalidQuery).ParseExecutableDocument());

        Assert.Contains("Expected", exception.Message);
    }

    #endregion

    #region Alias Parsing Compliance

    [Fact]
    public void Parse_FieldAliases_ShouldMatchSpecification()
    {
        // Given: Query with field aliases from GraphQL specification
        var query = @"query GetUsers {
            user {
                id
                firstName: name
                avatar: profilePic(size: 50)
                smallPic: profilePic(size: 25)
            }
        }";

        // When: Parse the query
        var document = Parser.Create(query).ParseExecutableDocument();

        // Then: Verify aliases
        var operation = document.OperationDefinitions.First();
        var userField = (FieldSelection)operation.SelectionSet.First();
        var selections = userField.SelectionSet.ToList();

        // firstName: name
        var firstNameField = (FieldSelection)selections[1];
        Assert.Equal("firstName", firstNameField.Alias);
        Assert.Equal("name", firstNameField.Name);

        // avatar: profilePic(size: 50)
        var avatarField = (FieldSelection)selections[2];
        Assert.Equal("avatar", avatarField.Alias);
        Assert.Equal("profilePic", avatarField.Name);
        Assert.Single(avatarField.Arguments);

        // smallPic: profilePic(size: 25)
        var smallPicField = (FieldSelection)selections[3];
        Assert.Equal("smallPic", smallPicField.Alias);
        Assert.Equal("profilePic", smallPicField.Name);
    }

    #endregion

    #region Multiple Operations Parsing Compliance

    [Fact]
    public void Parse_MultipleOperations_ShouldMatchSpecification()
    {
        // Given: Document with multiple operations from GraphQL specification
        var document = @"
            query GetUser($id: ID!) {
                user(id: $id) { name }
            }
            
            mutation UpdateUser($id: ID!, $input: UpdateUserInput!) {
                updateUser(id: $id, input: $input) { id name }
            }
            
            subscription OnUserChange($id: ID!) {
                userChanged(id: $id) { id name lastModified }
            }";

        // When: Parse the document
        var parsedDoc = Parser.Create(document).ParseExecutableDocument();

        // Then: Verify all operations
        Assert.Equal(3, parsedDoc.OperationDefinitions.Count);

        var operations = parsedDoc.OperationDefinitions.ToList();
        Assert.Equal(OperationType.Query, operations[0].Operation);
        Assert.Equal("GetUser", operations[0].Name);

        Assert.Equal(OperationType.Mutation, operations[1].Operation);
        Assert.Equal("UpdateUser", operations[1].Name);

        Assert.Equal(OperationType.Subscription, operations[2].Operation);
        Assert.Equal("OnUserChange", operations[2].Name);
    }

    #endregion
}