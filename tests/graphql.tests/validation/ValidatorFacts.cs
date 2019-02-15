using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLParser.AST;
using tanka.graphql.sdl;
using tanka.graphql.type;
using tanka.graphql.validation;
using tanka.graphql.validation.rules;
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

        private Task<ValidationResult> ValidateAsync(
            GraphQLDocument document,
            IValidationRule rule,
            Dictionary<string, object> variables = null)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            return Validator.ValidateAsync(
                Schema,
                document,
                variables,
                new[] {rule});
        }

        [Fact]
        public async Task Rule_511_Executable_Definitions()
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
            var result = await ValidateAsync(
                document,
                new R511ExecutableDefinitions());

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == Errors.R511ExecutableDefinitions);
        }
    }
}