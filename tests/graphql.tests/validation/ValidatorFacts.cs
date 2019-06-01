using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.sdl;
using tanka.graphql.type;
using tanka.graphql.validation;
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

                input ComplexInput { name: String!, owner: String }

                extend type Query {
                  findDog(complex: ComplexInput): Dog
                  booleanList(booleanListArg: [Boolean!]): Boolean
                }
                ";

            Schema = Sdl.Schema(Parser.ParseDocument(sdl));
        }

        public ISchema Schema { get; }

        private ValidationResult Validate(
            GraphQLDocument document,
            CombineRule rule,
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
                ExecutionRules.R511ExecutableDefinitions());

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
                ExecutionRules.R5211OperationNameUniqueness());

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
            var document = Parser.ParseDocument(
                @"{
                  dog {
                    name
                  }
                }");

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
                ExecutionRules.R5221LoneAnonymousOperation());

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
                ExecutionRules.R5231SingleRootField());

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
                ExecutionRules.R5231SingleRootField());

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
                ExecutionRules.R531FieldSelections());

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
                ExecutionRules.R531FieldSelections());

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
                ExecutionRules.R531FieldSelections());

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
                ExecutionRules.R531FieldSelections());

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
                ExecutionRules.R531FieldSelections());

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
                ExecutionRules.R531FieldSelections());

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
                ExecutionRules.R531FieldSelections());

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

        [Fact]
        public void Rule_532_Field_Selection_Merging_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"
                fragment mergeIdenticalFields on Dog {
                  name
                  name
                }

                fragment mergeIdenticalAliasesAndFields on Dog {
                  otherName: name
                  otherName: name
                }
                ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R532FieldSelectionMerging());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_532_Field_Selection_Merging_valid2()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"
                fragment mergeIdenticalFieldsWithIdenticalArgs on Dog {
                  doesKnowCommand(dogCommand: SIT)
                  doesKnowCommand(dogCommand: SIT)
                }

                fragment mergeIdenticalFieldsWithIdenticalValues on Dog {
                  doesKnowCommand(dogCommand: $dogCommand)
                  doesKnowCommand(dogCommand: $dogCommand)
                }
                ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R532FieldSelectionMerging(),
                new Dictionary<string, object>()
                {
                    ["dogCommand"] = "SIT"
                });

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_532_Field_Selection_Merging_valid3()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"
                fragment safeDifferingFields on Pet {
                  ... on Dog {
                    volume: barkVolume
                  }
                  ... on Cat {
                    volume: meowVolume
                  }
                }

                fragment safeDifferingArgs on Pet {
                  ... on Dog {
                    doesKnowCommand(dogCommand: SIT)
                  }
                  ... on Cat {
                    doesKnowCommand(catCommand: JUMP)
                  }
                }
                ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R532FieldSelectionMerging());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_532_Field_Selection_Merging_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"
                fragment conflictingBecauseAlias on Dog {
                  name: nickname
                  name
                }
                ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R532FieldSelectionMerging());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R532FieldSelectionMerging 
                         && error.Nodes.OfType<GraphQLFieldSelection>()
                             .Any(n => n.Name.Value == "name"));
        }
        
        [Fact]
        public void Rule_532_Field_Selection_Merging_invalid2()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"
                fragment conflictingArgsOnValues on Dog {
                  doesKnowCommand(dogCommand: SIT)
                  doesKnowCommand(dogCommand: HEEL)
                }

                fragment conflictingArgsValueAndVar on Dog {
                  doesKnowCommand(dogCommand: SIT)
                  doesKnowCommand(dogCommand: $dogCommand)
                }

                fragment conflictingArgsWithVars on Dog {
                  doesKnowCommand(dogCommand: $varOne)
                  doesKnowCommand(dogCommand: $varTwo)
                }

                fragment differingArgs on Dog {
                  doesKnowCommand(dogCommand: SIT)
                  doesKnowCommand
                }
                ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R532FieldSelectionMerging(),
                new Dictionary<string, object>()
                {
                    ["dogCommand"] = "HEEL",
                    ["varOne"] = "SIT",
                    ["varTwo"] = "HEEL"
                });

            /* Then */
            Assert.False(result.IsValid);
            Assert.All(
                result.Errors,
                error => Assert.True(error.Code == ValidationErrorCodes.R532FieldSelectionMerging));
        }

        [Fact]
        public void Rule_532_Field_Selection_Merging_invalid3()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"
                fragment conflictingDifferingResponses on Pet {
                  ... on Dog {
                    someValue: nickname
                  }
                  ... on Cat {
                    someValue: meowVolume
                  }
                }
                ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R532FieldSelectionMerging());

            /* Then */
            Assert.False(result.IsValid);
            Assert.All(
                result.Errors,
                error => Assert.True(error.Code == ValidationErrorCodes.R532FieldSelectionMerging));
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
                ExecutionRules.R533LeafFieldSelections());

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
                ExecutionRules.R533LeafFieldSelections());

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
                ExecutionRules.R533LeafFieldSelections());

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
                ExecutionRules.R533LeafFieldSelections());

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
                ExecutionRules.R533LeafFieldSelections());

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
                ExecutionRules.R541ArgumentNames());

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
                ExecutionRules.R541ArgumentNames());

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
                ExecutionRules.R541ArgumentNames());

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
                ExecutionRules.R542ArgumentUniqueness());

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
                ExecutionRules.R542ArgumentUniqueness());

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
                ExecutionRules.R542ArgumentUniqueness());

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
                ExecutionRules.R5421RequiredArguments());

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
                ExecutionRules.R5421RequiredArguments());

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
                ExecutionRules.R5421RequiredArguments());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5421RequiredArguments);
        }

        [Fact]
        public void Rule_5511_Fragment_Name_Uniqueness_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                      dog {
                        ...fragmentOne
                        ...fragmentTwo
                      }
                    }

                    fragment fragmentOne on Dog {
                      name
                    }

                    fragment fragmentTwo on Dog {
                      owner {
                        name
                      }
                    }
                  ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5511FragmentNameUniqueness());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5511_Fragment_Name_Uniqueness_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                      dog {
                        ...fragmentOne
                      }
                    }

                    fragment fragmentOne on Dog {
                      name
                    }

                    fragment fragmentOne on Dog {
                      owner {
                        name
                      }
                    }");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5511FragmentNameUniqueness());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5511FragmentNameUniqueness);
        }

        [Fact]
        public void Rule_5512_Fragment_Spread_Type_Existence_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment correctType on Dog {
                      name
                    }

                    fragment inlineFragment on Dog {
                      ... on Dog {
                        name
                      }
                    }

                    fragment inlineFragment2 on Dog {
                      ... @include(if: true) {
                        name
                      }
                    }
                  ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5512FragmentSpreadTypeExistence());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5512_Fragment_Spread_Type_Existence_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment notOnExistingType on NotInSchema {
                      name
                    }"
                );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5512FragmentSpreadTypeExistence());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5512FragmentSpreadTypeExistence);
        }

        [Fact]
        public void Rule_5512_Fragment_Spread_Type_Existence_invalid2()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment inlineNotExistingType on Dog {
                      ... on NotInSchema {
                        name
                      }
                    }"
            );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5512FragmentSpreadTypeExistence());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5512FragmentSpreadTypeExistence);
        }

        [Fact]
        public void Rule_5513_FragmentsOnCompositeTypes_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment fragOnObject on Dog {
                      name
                    }

                    fragment fragOnInterface on Pet {
                      name
                    }

                    fragment fragOnUnion on CatOrDog {
                      ... on Dog {
                        name
                      }
                    }
                  ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5513FragmentsOnCompositeTypes());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5513_FragmentsOnCompositeTypes_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment fragOnScalar on Int {
                      something
                    }"
            );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5513FragmentsOnCompositeTypes());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5513FragmentsOnCompositeTypes);
        }

        [Fact]
        public void Rule_5513_FragmentsOnCompositeTypes_invalid2()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment inlineFragOnScalar on Dog {
                      ... on Boolean {
                        somethingElse
                      }
                    }"
            );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5513FragmentsOnCompositeTypes());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5513FragmentsOnCompositeTypes);
        }

        [Fact]
        public void Rule_5514_FragmentsMustBeUsed_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment nameFragment on Dog {
                      name
                    }

                    {
                      dog {
                        ...nameFragment
                      }
                    }"
            );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5514FragmentsMustBeUsed());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5514_FragmentsMustBeUsed_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment nameFragment on Dog {
                      name
                    }

                    {
                      dog {
                        name
                      }
                    }"
                );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5514FragmentsMustBeUsed());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5514FragmentsMustBeUsed);
        }

        [Fact]
        public void Rule_5522_FragmentSpreadsMustNotFormCycles_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                      dog {
                        ...nameFragment
                      }
                    }

                    fragment nameFragment on Dog {
                      name
                      ...barkVolumeFragment
                    }

                    fragment barkVolumeFragment on Dog {
                      barkVolume
                      ...nameFragment
                    }"
            );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5522FragmentSpreadsMustNotFormCycles());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Contains(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5522FragmentSpreadsMustNotFormCycles);
        }

        [Fact]
        public void Rule_5522_FragmentSpreadsMustNotFormCycles_invalid2()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                      dog {
                        ...dogFragment
                      }
                    }

                    fragment dogFragment on Dog {
                      name
                      owner {
                        ...ownerFragment
                      }
                    }

                    fragment ownerFragment on Dog {
                      name
                      pets {
                        ...dogFragment
                      }
                    }"
            );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5522FragmentSpreadsMustNotFormCycles());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Contains(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5522FragmentSpreadsMustNotFormCycles);
        }

        [Fact]
        public void Rule_5523_FragmentSpreadIsPossible_in_scope_valid()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment dogFragment on Dog {
                      ... on Dog {
                        barkVolume
                      }
                    }"
            );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5523FragmentSpreadIsPossible());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5523_FragmentSpreadIsPossible_in_scope_invalid()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment catInDogFragmentInvalid on Dog {
                  ... on Cat {
                    meowVolume
                  }
                }"
            );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5523FragmentSpreadIsPossible());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5523FragmentSpreadIsPossible);
        }

        [Fact]
        public void Rule_5523_FragmentSpreadIsPossible_in_abstract_scope_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment petNameFragment on Pet {
                      name
                    }

                    fragment interfaceWithinObjectFragment on Dog {
                      ...petNameFragment
                    }
                    ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5523FragmentSpreadIsPossible());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5523_FragmentSpreadIsPossible_in_abstract_scope_valid2()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment catOrDogNameFragment on CatOrDog {
                      ... on Cat {
                        meowVolume
                      }
                    }

                    fragment unionWithObjectFragment on Dog {
                      ...catOrDogNameFragment
                    }
                    ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5523FragmentSpreadIsPossible());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5523_FragmentSpreadIsPossible_abstract_in_abstract_scope_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment unionWithInterface on Pet {
                      ...dogOrHumanFragment
                    }

                    fragment dogOrHumanFragment on DogOrHuman {
                      ... on Dog {
                        barkVolume
                      }
                    }
                    ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5523FragmentSpreadIsPossible());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_5523_FragmentSpreadIsPossible_abstract_in_abstract_scope_invalid()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment nonIntersectingInterfaces on Pet {
                      ...sentientFragment
                    }

                    fragment sentientFragment on Sentient {
                      name
                    }"
            );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R5523FragmentSpreadIsPossible());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R5523FragmentSpreadIsPossible);
        }

        [Fact]
        public void Rule_561_ValuesOfCorrectType_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment goodBooleanArg on Arguments {
                      booleanArgField(booleanArg: true)
                    }

                    fragment coercedIntIntoFloatArg on Arguments {
                      # Note: The input coercion rules for Float allow Int literals.
                      floatArgField(floatArg: 123)
                    }

                    query goodComplexDefaultValue($search: ComplexInput = { name: ""Fido"" }) {
                      findDog(complex: $search)
                    }
                  ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R561ValuesOfCorrectType());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_561_ValuesOfCorrectType_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"fragment stringIntoInt on Arguments {
                      intArgField(intArg: ""123"")
                    }"
            );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R561ValuesOfCorrectType());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R561ValuesOfCorrectType);
        }

        [Fact]
        public void Rule_561_ValuesOfCorrectType_invalid2()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"query badComplexValue {
                      findDog(complex: { name: 123 })
                    }"
            );

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R561ValuesOfCorrectType());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R561ValuesOfCorrectType);
        }

        [Fact]
        public void Rule_562_InputObjectFieldNames_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                      findDog(complex: { name: ""Fido"" })
                    }
                  ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R562InputObjectFieldNames());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_562_InputObjectFieldNames_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                      findDog(complex: { favoriteCookieFlavor: ""Bacon"" })
                    }
                    ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R562InputObjectFieldNames());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R562InputObjectFieldNames);
        }

        [Fact]
        public void Rule_563_InputObjectFieldUniqueness_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                      field(arg: { field: true, field: false })
                    }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R563InputObjectFieldUniqueness());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Contains(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R563InputObjectFieldUniqueness);
        }

        [Fact]
        public void Rule_564_InputObjectRequiredFields_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                       findDog(complex: { owner: ""Fido"" })
                  }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R564InputObjectRequiredFields());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R564InputObjectRequiredFields);
        }

        [Fact]
        public void Rule_564_InputObjectRequiredFields_invalid2()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                       findDog(complex: { name: null })
                  }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R564InputObjectRequiredFields());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R564InputObjectRequiredFields);
        }

        [Fact]
        public void Rule_57_DirectivesAreDefined_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                       findDog(complex: { name: ""Fido"" }) @skip(if: false)
                  }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R571And573Directives());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_571_DirectivesAreDefined_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                       findDog(complex: { name: ""Fido"" }) @doesNotExists
                  }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R571And573Directives());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R571DirectivesAreDefined);
        }

        [Fact]
        public void Rule_572_DirectivesAreInValidLocations_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"
                query {
                  field @skip(if: $foo)
                }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R572DirectivesAreInValidLocations());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_572_DirectivesAreInValidLocations_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"
                query @skip(if: $foo) {
                  field
                }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R572DirectivesAreInValidLocations());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R572DirectivesAreInValidLocations);
        }

        [Fact]
        public void Rule_573_DirectivesAreUniquePerLocation_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"query ($foo: Boolean = true, $bar: Boolean = false) {
                      field @skip(if: $foo) {
                        subfieldA
                      }
                      field @skip(if: $bar) {
                        subfieldB
                      }
                    }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R571And573Directives());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_573_DirectivesAreUniquePerLocation_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"query ($foo: Boolean = true, $bar: Boolean = false) {
                      field @skip(if: $foo) @skip(if: $bar)
                    }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R571And573Directives());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R573DirectivesAreUniquePerLocation);
        }

        [Fact]
        public void Rule_58_Variables_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"query A($atOtherHomes: Boolean) {
                      ...HouseTrainedFragment
                    }

                    query B($atOtherHomes: Boolean) {
                      ...HouseTrainedFragment
                    }

                    fragment HouseTrainedFragment on Query {
                      dog {
                        isHousetrained(atOtherHomes: $atOtherHomes)
                      }
                    }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R58Variables());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_58_Variables_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"query houseTrainedQuery($atOtherHomes: Boolean, $atOtherHomes: Boolean) {
                      dog {
                        isHousetrained(atOtherHomes: $atOtherHomes)
                      }
                    }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R58Variables());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R58Variables);
        }

        [Fact]
        public void Rule_582_VariablesAreInputTypes_valid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"query takesBoolean($atOtherHomes: Boolean) {
                      dog {
                        isHousetrained(atOtherHomes: $atOtherHomes)
                      }
                    }

                    query takesComplexInput($complexInput: ComplexInput) {
                      findDog(complex: $complexInput) {
                        name
                      }
                    }

                    query TakesListOfBooleanBang($booleans: [Boolean!]) {
                      booleanList(booleanListArg: $booleans)
                    }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R58Variables());

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Rule_582_VariablesAreInputTypes_invalid1()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"query takesCat($cat: Cat) {
                      __typename
                    }

                    query takesDogBang($dog: Dog!) {
                      __typename
                    }

                    query takesListOfPet($pets: [Pet]) {
                      __typename
                    }

                    query takesCatOrDog($catOrDog: CatOrDog) {
                      __typename
                    }
                 ");

            /* When */
            var result = Validate(
                document,
                ExecutionRules.R58Variables());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Equal(4, result.Errors.Count());
            Assert.Contains(
                result.Errors,
                error => error.Code == ValidationErrorCodes.R58Variables
                         && error.Message.StartsWith("Variables can only be input types. Objects, unions,"));
        }

        [Fact(Skip = "TODO")]
        public void Rule_583_AllVariableUsesDefined()
        {

        }

        [Fact(Skip = "TODO")]
        public void Rule_584_AllVariablesUsed_valid1()
        {
        }

        [Fact(Skip = "TODO")]
        public void Rule_585_AllVariableUsagesAreAllowed_valid1()
        {
        }
    }
}