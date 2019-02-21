using System;
using System.Collections.Generic;
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
                @"type Query {
                  dog: Dog
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
        public void Rule_5221_Lone_Anonymous_Operation_invalid_valid()
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
    }
}