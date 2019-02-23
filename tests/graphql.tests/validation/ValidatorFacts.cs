using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.sdl;
using tanka.graphql.type;
using tanka.graphql.validation;
using tanka.graphql.validation.rules2;
using Xunit;

namespace tanka.graphql.tests.validation
{
    public class ValidatorFacts
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
                  optionalNonNullBooleanArgField(optionalBooleanArg: Boolean! = false): Boolean!
                }

                extend type Query {
                  arguments: Arguments
                }
                ";

            Schema = Sdl.Schema(Parser.ParseDocument(sdl));
        }

        public ISchema Schema { get; }

        private ValidationResult Validate(
            GraphQLDocument document,
            IRule rule,
            Dictionary<string, object> variables = null)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            return Validator.Validate(           
                new[] {rule},
                Schema,
                document,
                variables);
        }

        [Fact]
        public void Rule_511_Executable_Definitions()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"query getDogName {
                  dog {
                    name
                    color
                  }
                }

                extend type Dog {
                  color: String
                }");

            /* When */
            var result = Validate(
                document,
                new R511ExecutableDefinitions());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R511ExecutableDefinitions);
        }

        [Fact]
        public void Rule_5211_Operation_Name_Uniqueness_valid()
        {
            /* Given */
            var document = Parser.ParseDocument(
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
                    }");

            /* When */
            var result = Validate(
                document,
                new R5211OperationNameUniqueness());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5211_Operation_Name_Uniqueness_invalid()
        {
            /* Given */
            var document = Parser.ParseDocument(
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
                    }");

            /* When */
            var result = Validate(
                document,
                new R5211OperationNameUniqueness());

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
            var document = Parser.ParseDocument(
                @"{
                  dog {
                    name
                  }
                }");

            /* When */
            var result = Validate(
                document,
                new R5221LoneAnonymousOperation());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5221_Lone_Anonymous_Operation_invalid()
        {
            /* Given */
            var document = Parser.ParseDocument(
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
                }");

            /* When */
            var result = Validate(
                document,
                new R5221LoneAnonymousOperation());

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
            var document = Parser.ParseDocument(
                @"subscription sub {
                      newMessage {
                        body
                        sender
                      }
                    }");

            /* When */
            var result = Validate(
                document,
                new R5221LoneAnonymousOperation());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5231_Single_root_field_valid_with_fragment()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"subscription sub {
                      ...newMessageFields
                    }

                    fragment newMessageFields on Subscription {
                      newMessage {
                        body
                        sender
                      }
                    }");

            /* When */
            var result = Validate(
                document,
                new R5231SingleRootField());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5231_Single_root_field_invalid()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"subscription sub {
                      newMessage {
                        body
                        sender
                      }
                      disallowedSecondRootField
                    }");

            /* When */
            var result = Validate(
                document,
                new R5231SingleRootField());

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
            var document = Parser.ParseDocument(
                @"subscription sub {
                      ...multipleSubscriptions
                    }

                    fragment multipleSubscriptions on Subscription {
                      newMessage {
                        body
                        sender
                      }
                      disallowedSecondRootField
                    }");

            /* When */
            var result = Validate(
                document,
                new R5231SingleRootField());

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
            var document = Parser.ParseDocument(
                @"subscription sub {
                      newMessage {
                        body
                        sender
                      }
                      __typename
                    }");

            /* When */
            var result = Validate(
                document,
                new R5231SingleRootField());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5231SingleRootField);
        }

        [Fact]
        public void Rule_531_Field_Selections_invalid_with_fragment()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment fieldNotDefined on Dog {
                      meowVolume
                    }");

            /* When */
            var result = Validate(
                document,
                new R531FieldSelections());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R531FieldSelections);
        }

        [Fact]
        public void Rule_531_Field_Selections_invalid_with_alias()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment aliasedLyingFieldTargetNotDefined on Dog {
                      barkVolume: kawVolume
                    }");

            /* When */
            var result = Validate(
                document,
                new R531FieldSelections());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R531FieldSelections);
        }

        [Fact]
        public void Rule_531_Field_Selections_valid()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                  dog {
                    name
                  }
                }");

            /* When */
            var result = Validate(
                document,
                new R531FieldSelections());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_531_Field_Selections_valid_with_interface()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment interfaceFieldSelection on Pet {
                  name
                }");

            /* When */
            var result = Validate(
                document,
                new R531FieldSelections());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_531_Field_Selections_invalid_with_interface()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment definedOnImplementorsButNotInterface on Pet {
                      nickname
                    }");

            /* When */
            var result = Validate(
                document,
                new R531FieldSelections());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R531FieldSelections);
        }

        [Fact]
        public void Rule_531_Field_Selections_valid_with_union()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment inDirectFieldSelectionOnUnion on CatOrDog {
                  __typename
                  ... on Pet {
                    name
                  }
                  ... on Dog {
                    barkVolume
                  }
                }");

            /* When */
            var result = Validate(
                document,
                new R531FieldSelections());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_531_Field_Selections_invalid_with_union()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment directFieldSelectionOnUnion on CatOrDog {
                      name
                      barkVolume
                    }");

            /* When */
            var result = Validate(
                document,
                new R531FieldSelections());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R531FieldSelections 
                         && error.Nodes.OfType<GraphQLFieldSelection>()
                             .Any(n => n.Name.Value == "name"));

            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R531FieldSelections 
                         && error.Nodes.OfType<GraphQLFieldSelection>()
                             .Any(n => n.Name.Value == "barkVolume"));
        }

        [Fact(Skip = "Not implemented")]
        public void Rule_532_Field_Selection_Merging()
        {
            //todo
        }

        [Fact]
        public void Rule_533_Leaf_Field_Selections_valid()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment scalarSelection on Dog {
                      barkVolume
                    }");

            /* When */
            var result = Validate(
                document,
                new R533LeafFieldSelections());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_533_Leaf_Field_Selections_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment scalarSelectionsNotAllowedOnInt on Dog {
                      barkVolume {
                        sinceWhen
                      }
                    }");

            /* When */
            var result = Validate(
                document,
                new R533LeafFieldSelections());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R533LeafFieldSelections);
        }

        [Fact]
        public void Rule_533_Leaf_Field_Selections_invalid2()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"query directQueryOnObjectWithoutSubFields {
                      human
                    }");

            /* When */
            var result = Validate(
                document,
                new R533LeafFieldSelections());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R533LeafFieldSelections);
        }

        [Fact]
        public void Rule_533_Leaf_Field_Selections_invalid3()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"query directQueryOnInterfaceWithoutSubFields {
                      pet
                    }");

            /* When */
            var result = Validate(
                document,
                new R533LeafFieldSelections());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R533LeafFieldSelections);
        }

        [Fact]
        public void Rule_533_Leaf_Field_Selections_invalid4()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"query directQueryOnUnionWithoutSubFields {
                      catOrDog
                    }");

            /* When */
            var result = Validate(
                document,
                new R533LeafFieldSelections());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R533LeafFieldSelections);
        }

        [Fact]
        public void Rule_541_Argument_Names_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment argOnRequiredArg on Dog {
                      doesKnowCommand(dogCommand: SIT)
                    }

                    fragment argOnOptional on Dog {
                      isHousetrained(atOtherHomes: true) @include(if: true)
                    }");

            /* When */
            var result = Validate(
                document,
                new R541ArgumentNames());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_541_Argument_Names_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment invalidArgName on Dog {
                      doesKnowCommand(command: CLEAN_UP_HOUSE)
                    }");

            /* When */
            var result = Validate(
                document,
                new R541ArgumentNames());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R541ArgumentNames);
        }

        [Fact]
        public void Rule_541_Argument_Names_invalid2()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment invalidArgName on Dog {
                      isHousetrained(atOtherHomes: true) @include(unless: false)
                    }");

            /* When */
            var result = Validate(
                document,
                new R541ArgumentNames());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R541ArgumentNames);
        }

        [Fact]
        public void Rule_542_Argument_Uniqueness_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment argOnRequiredArg on Dog {
                      doesKnowCommand(dogCommand: SIT)
                    }");

            /* When */
            var result = Validate(
                document,
                new R542ArgumentUniqueness());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_542_Argument_Uniqueness_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment invalidArgName on Dog {
                      doesKnowCommand(command: SIT, command: SIT)
                    }");

            /* When */
            var result = Validate(
                document,
                new R542ArgumentUniqueness());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R542ArgumentUniqueness);
        }

        [Fact]
        public void Rule_542_Argument_Uniqueness_invalid2()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment invalidArgName on Dog {
                      doesKnowCommand(command: SIT) @skip(if: true, if: true)
                    }");

            /* When */
            var result = Validate(
                document,
                new R542ArgumentUniqueness());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R542ArgumentUniqueness);
        }

        [Fact]
        public void Rule_5421_Required_Arguments_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment goodBooleanArg on Arguments {
                      booleanArgField(booleanArg: true)
                    }

                    fragment goodNonNullArg on Arguments {
                      nonNullBooleanArgField(nonNullBooleanArg: true)
                    }

                    fragment goodBooleanArgDefault on Arguments {
                      booleanArgField
                    }
                    ");

            /* When */
            var result = Validate(
                document,
                new R5421RequiredArguments());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5421_Required_Arguments_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment missingRequiredArg on Arguments {
                      nonNullBooleanArgField
                    }");

            /* When */
            var result = Validate(
                document,
                new R5421RequiredArguments());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5421RequiredArguments);
        }

        [Fact]
        public void Rule_5421_Required_Arguments_invalid2()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment missingRequiredArg on Arguments {
                      nonNullBooleanArgField(nonNullBooleanArg: null)
                    }");

            /* When */
            var result = Validate(
                document,
                new R5421RequiredArguments());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5421RequiredArguments);
        }
    }
}