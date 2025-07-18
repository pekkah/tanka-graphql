using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public partial class ValidatorFacts
{
    public ValidatorFacts()
    {
        var sdl =
            @"
                schema {
                    query: Query
                    subscription: Subscription
                }

                type Query {
                  dog: Dog
                  human: Human
                  pet: Pet
                  catOrDog: CatOrDog
                }

                type Subscription {
                    newMessage: Message
                }

                type Message {
                    body: String
                    sender: String
                }

                enum DogCommand { SIT, DOWN, HEEL }

                type Dog implements Pet {
                  name: String!
                  nickname: String
                  barkVolume: Int
                  doesKnowCommand(dogCommand: DogCommand!): Boolean!
                  isHousetrained(atOtherHomes: Boolean): Boolean!
                  owner: Human
                }

                interface Sentient {
                  name: String!
                }

                interface Pet {
                  name: String!
                }

                type Alien implements Sentient {
                  name: String!
                  homePlanet: String
                }

                type Human implements Sentient {
                  name: String!
                }

                enum CatCommand { JUMP }

                type Cat implements Pet {
                  name: String!
                  nickname: String
                  doesKnowCommand(catCommand: CatCommand!): Boolean!
                  meowVolume: Int
                }

                union CatOrDog = Cat | Dog
                union DogOrHuman = Dog | Human
                union HumanOrAlien = Human | Alien

                type Arguments {
                  multipleReqs(x: Int!, y: Int!): Int!
                  booleanArgField(booleanArg: Boolean): Boolean
                  floatArgField(floatArg: Float): Float
                  intArgField(intArg: Int): Int
                  nonNullBooleanArgField(nonNullBooleanArg: Boolean!): Boolean!
                  booleanListArgField(booleanListArg: [Boolean]!): [Boolean]
                  nonNullBooleanListField(nonNullBooleanListArg: [Boolean]!): [Boolean]
                  optionalNonNullBooleanArgField(optionalBooleanArg: Boolean! = false): Boolean!
                }

                extend type Query {
                  arguments: Arguments
                }

                input ComplexInput { name: String!, owner: String }

                extend type Query {
                  findDog(complex: ComplexInput): Dog
                  booleanList(booleanListArg: [Boolean!]): Boolean
                }

            interface Node {
              id: ID!
            }

            interface Resource implements Node {
              id: ID!
              url: String
            }
                ";

        Schema = new SchemaBuilder()
            .Add(sdl)
            .Build(new SchemaBuildOptions()).Result;
    }

    public ISchema Schema { get; }

    [Fact(Skip = "Not required by new language module")]
    public void Rule_511_Executable_Definitions()
    {
        /* Given */
        /*var document = Parser.ParseDocument(
            @"query getDogName {
              dog {
                name
                color
              }
            }

            extend type Dog {
              color: String
            }");*/

        /* When */
        /*var result = Validate(
            document,
            ExecutionRules.R511ExecutableDefinitions());*/

        /* Then */
        /*Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R511ExecutableDefinitions);*/
    }

    [Fact]
    public void Rule_5211_Operation_Name_Uniqueness_valid()
    {
        /* Given */
        var document =
            @"query getDogName {
                      dog {
                        name
                      }
                    }

                    query getOwnerName {
                      dog {
                        owner {
                          name
                        }
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5211OperationNameUniqueness());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5211_Operation_Name_Uniqueness_invalid()
    {
        /* Given */
        var document =
            @"query getName {
                      dog {
                        name
                      }
                    }

                    query getName {
                      dog {
                        owner {
                          name
                        }
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5211OperationNameUniqueness());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5211OperationNameUniqueness);
    }

    [Fact]
    public void Rule_5221_Lone_Anonymous_Operation_valid()
    {
        /* Given */
        var document =
            @"{
                  dog {
                    name
                  }
                }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5221LoneAnonymousOperation());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5221_Lone_Anonymous_Operation_invalid()
    {
        /* Given */
        var document =
            @"{
                  dog {
                    name
                  }
                }

                query getName {
                  dog {
                    owner {
                      name
                    }
                  }
                }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5221LoneAnonymousOperation());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5221LoneAnonymousOperation);
    }

    [Fact]
    public void Rule_5231_Single_root_field_valid()
    {
        /* Given */
        var document =
            @"subscription sub {
                      newMessage {
                        body
                        sender
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5221LoneAnonymousOperation());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5231_Single_root_field_valid_with_fragment()
    {
        /* Given */
        var document =
            @"subscription sub {
                      ...newMessageFields
                    }

                    fragment newMessageFields on Subscription {
                      newMessage {
                        body
                        sender
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5231SingleRootField());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5231_Single_root_field_invalid()
    {
        /* Given */
        var document =
            @"subscription sub {
                      newMessage {
                        body
                        sender
                      }
                      disallowedSecondRootField
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5231SingleRootField());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5231SingleRootField);
    }

    [Fact]
    public void Rule_5231_Single_root_field_invalid_with_fragment()
    {
        /* Given */
        var document =
            @"subscription sub {
                      ...multipleSubscriptions
                    }

                    fragment multipleSubscriptions on Subscription {
                      newMessage {
                        body
                        sender
                      }
                      disallowedSecondRootField
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5231SingleRootField());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5231SingleRootField);
    }

    [Fact]
    public void Rule_5231_Single_root_field_invalid_with_typename()
    {
        /* Given */
        var document =
            @"subscription sub {
                      newMessage {
                        body
                        sender
                      }
                      __typename
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5231SingleRootField());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5231SingleRootField);
    }

    private ValidationResult Validate(
        ExecutableDocument document,
        CombineRule rule,
        Dictionary<string, object> variables = null)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (rule == null) throw new ArgumentNullException(nameof(rule));

        return Validator.Validate(
            new[] { rule },
            Schema,
            document,
            variables);
    }
}