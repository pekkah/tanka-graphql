using tanka.graphql.type;
using tanka.graphql.typeSystem;

namespace tanka.graphql.tests.data.starwars
{
    public class StarwarsSchema
    {
        public static ISchema BuildSchema()
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

            builder
                .Field(Character, "id", ScalarType.NonNullString)
                .Field(Character, "name", ScalarType.NonNullString)
                .Field(Character, "friends", CharacterList)
                .Field(Character, "appearsIn", EpisodeList);

            builder.Object("Human", out var Human,
                    "Human character",
                    new[] {Character})
                .Field(Human, "id", ScalarType.NonNullString)
                .Field(Human, "name", ScalarType.NonNullString)
                .Field(Human, "friends", CharacterList)
                .Field(Human, "appearsIn", EpisodeList)
                .Field(Human, "homePlanet", ScalarType.String);

            builder.Query(out var Query)
                .Field(Query, "human", Human,
                    args: ("id", ScalarType.NonNullString, default, default))
                .Field(Query, "character", Character,
                    args: ("id", ScalarType.NonNullString, default, default))
                .Field(Query, "characters", CharacterList);


            builder.InputObject("HumanInput", out var HumanInput)
                .InputField(HumanInput, "name", ScalarType.NonNullString);

            builder.Mutation(out var Mutation)
                .Field(Mutation, "addHuman", Human,
                    args: ("human", HumanInput, default, default));

            var schema = builder.Build();

            return schema;
        }
    }
}