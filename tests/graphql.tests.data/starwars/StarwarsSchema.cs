using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Tests.Data.Starwars
{
    public class StarwarsSchema
    {
        public static SchemaBuilder Create()
        {
            var builder = new SchemaBuilder();

            var Episode = new EnumType("Episode", new EnumValues
            {
                ["NEWHOPE"] = null,
                ["EMPIRE"] = null,
                ["JEDI"] = null
            });

            builder.Include(Episode);

            var EpisodeList = new List(Episode);

            builder.Interface("Character", out var Character,
                "Character in the movie");

            // use NamedTypeReference as proxy to bypass circular dependencies
            var CharacterList = new List(Character);

            builder.Connections(connect => connect
                .Field(Character, "id", ScalarType.NonNullString)
                .Field(Character, "name", ScalarType.NonNullString)
                .Field(Character, "friends", CharacterList)
                .Field(Character, "appearsIn", EpisodeList));

            builder.Object("Human", out var Human,
                    "Human character",
                    new[] {Character})
                .Connections(connect => connect
                    .Field(Human, "id", ScalarType.NonNullString)
                    .Field(Human, "name", ScalarType.NonNullString)
                    .Field(Human, "friends", CharacterList)
                    .Field(Human, "appearsIn", EpisodeList)
                    .Field(Human, "homePlanet", ScalarType.String));

            builder.Query(out var Query)
                .Connections(connect => connect
                    .Field(Query, "human", Human,
                        args: args => args.Arg("id", ScalarType.NonNullString, default, default))
                    .Field(Query, "character", Character,
                        args: args => args.Arg("id", ScalarType.NonNullString, default, default))
                    .Field(Query, "characters", CharacterList));


            builder.InputObject("HumanInput", out var HumanInput)
                .Connections(connect => connect
                    .InputField(HumanInput, "name", ScalarType.NonNullString));

            builder.Mutation(out var Mutation)
                .Connections(connect => connect
                    .Field(Mutation, "addHuman", Human,
                        args: args => args.Arg("human", HumanInput, default, default)));

            return builder;
        }
    }
}