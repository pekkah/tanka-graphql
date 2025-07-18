using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public partial class ValidatorFacts
{
    /// <summary>
    /// Custom scalar type validation tests
    /// </summary>
    [Fact]
    public void Rule_561_ValuesOfCorrectType_custom_scalar_valid()
    {
        /* Given */
        var customScalarSchema = new SchemaBuilder()
            .Add(@"
                scalar CustomScalar
                
                type Query {
                  customField(customArg: CustomScalar): String
                }
                
                type CustomType {
                  id: ID!
                  value: CustomScalar
                }
                
                input CustomInput {
                  customField: CustomScalar
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query customScalarQuery {
              customField(customArg: ""custom_value"")
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R561ValuesOfCorrectType() },
            customScalarSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_561_ValuesOfCorrectType_custom_scalar_in_input_object_valid()
    {
        /* Given */
        var customScalarSchema = new SchemaBuilder()
            .Add(@"
                scalar URL
                scalar DateTime
                
                type Query {
                  searchByFilter(filter: SearchFilter): [Result]
                }
                
                type Result {
                  id: ID!
                  url: URL
                  createdAt: DateTime
                }
                
                input SearchFilter {
                  url: URL
                  createdAfter: DateTime
                  limit: Int
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query searchWithCustomScalars {
              searchByFilter(filter: {
                url: ""https://example.com"",
                createdAfter: ""2023-01-01T00:00:00Z"",
                limit: 10
              }) {
                id
                url
                createdAt
              }
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R561ValuesOfCorrectType() },
            customScalarSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_561_ValuesOfCorrectType_custom_scalar_with_variables_valid()
    {
        /* Given */
        var customScalarSchema = new SchemaBuilder()
            .Add(@"
                scalar JSON
                
                type Query {
                  processData(data: JSON): String
                }
                
                input DataInput {
                  jsonData: JSON
                  metadata: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query processWithVariable($jsonVar: JSON) {
              processData(data: $jsonVar)
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R561ValuesOfCorrectType() },
            customScalarSchema,
            Parser.ParseExecutableDocument(document),
            new Dictionary<string, object> { ["jsonVar"] = "{\"key\": \"value\"}" });

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_561_ValuesOfCorrectType_custom_scalar_list_valid()
    {
        /* Given */
        var customScalarSchema = new SchemaBuilder()
            .Add(@"
                scalar UUID
                
                type Query {
                  getItems(ids: [UUID!]!): [Item]
                }
                
                type Item {
                  id: UUID!
                  name: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query getItemsByIds {
              getItems(ids: [
                ""550e8400-e29b-41d4-a716-446655440000"",
                ""550e8400-e29b-41d4-a716-446655440001""
              ]) {
                id
                name
              }
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R561ValuesOfCorrectType() },
            customScalarSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_561_ValuesOfCorrectType_custom_scalar_nested_in_complex_input_valid()
    {
        /* Given */
        var customScalarSchema = new SchemaBuilder()
            .Add(@"
                scalar PhoneNumber
                scalar Email
                scalar Date
                
                type Query {
                  createUser(input: CreateUserInput!): User
                }
                
                type User {
                  id: ID!
                  profile: UserProfile
                }
                
                type UserProfile {
                  email: Email
                  phone: PhoneNumber
                  birthDate: Date
                }
                
                input CreateUserInput {
                  profile: UserProfileInput!
                  preferences: UserPreferencesInput
                }
                
                input UserProfileInput {
                  email: Email!
                  phone: PhoneNumber
                  birthDate: Date
                }
                
                input UserPreferencesInput {
                  notifications: Boolean
                  theme: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            mutation createUserWithCustomScalars {
              createUser(input: {
                profile: {
                  email: ""user@example.com"",
                  phone: ""+1-555-123-4567"",
                  birthDate: ""1990-01-01""
                },
                preferences: {
                  notifications: true,
                  theme: ""dark""
                }
              }) {
                id
                profile {
                  email
                  phone
                  birthDate
                }
              }
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R561ValuesOfCorrectType() },
            customScalarSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_561_ValuesOfCorrectType_custom_scalar_with_null_value_valid()
    {
        /* Given */
        var customScalarSchema = new SchemaBuilder()
            .Add(@"
                scalar OptionalData
                
                type Query {
                  processOptional(data: OptionalData): String
                }
                
                input OptionalInput {
                  optionalField: OptionalData
                  requiredField: String!
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query processWithNull {
              processOptional(data: null)
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R561ValuesOfCorrectType() },
            customScalarSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_561_ValuesOfCorrectType_custom_scalar_non_null_validation()
    {
        /* Given */
        var customScalarSchema = new SchemaBuilder()
            .Add(@"
                scalar CustomID
                
                type Query {
                  getById(id: CustomID!): Item
                }
                
                type Item {
                  id: CustomID!
                  value: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query getByIdWithNull {
              getById(id: null)
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R561ValuesOfCorrectType() },
            customScalarSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R561ValuesOfCorrectType);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_custom_scalar_variable_coercion_valid()
    {
        /* Given */
        var customScalarSchema = new SchemaBuilder()
            .Add(@"
                scalar CustomScalar
                
                type Query {
                  processCustom(value: CustomScalar!): String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query processCustomScalar($customVar: CustomScalar!) {
              processCustom(value: $customVar)
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R585AllVariableUsagesAreAllowed() },
            customScalarSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_custom_scalar_variable_coercion_invalid()
    {
        /* Given */
        var customScalarSchema = new SchemaBuilder()
            .Add(@"
                scalar CustomScalar
                
                type Query {
                  processCustom(value: CustomScalar!): String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query processCustomScalar($customVar: CustomScalar) {
              processCustom(value: $customVar)
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R585AllVariableUsagesAreAllowed() },
            customScalarSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R585AllVariableUsagesAreAllowed);
    }
}