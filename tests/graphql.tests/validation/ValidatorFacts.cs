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
                union HumanOrAlien = Human | Alien";

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
                error => error.Code == Errors.R511ExecutableDefinitions);
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
                error => error.Code == Errors.R5211OperationNameUniqueness);
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
                error => error.Code == Errors.R5221LoneAnonymousOperation);
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
                error => error.Code == Errors.R5231SingleRootField);
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
                error => error.Code == Errors.R5231SingleRootField);
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
                error => error.Code == Errors.R5231SingleRootField);
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
                error => error.Code == Errors.R531FieldSelections);
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
                error => error.Code == Errors.R531FieldSelections);
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
                error => error.Code == Errors.R531FieldSelections);
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
                error => error.Code == Errors.R531FieldSelections 
                         && error.Nodes.OfType<GraphQLFieldSelection>()
                             .Any(n => n.Name.Value == "name"));

            Assert.Single(
                result.Errors,
                error => error.Code == Errors.R531FieldSelections 
                         && error.Nodes.OfType<GraphQLFieldSelection>()
                             .Any(n => n.Name.Value == "barkVolume"));
        }
    }
}