using tanka.graphql.type;

namespace tanka.graphql.tests.data.starwars
{
    public class StarwarsSchema
    {
        public static Schema BuildSchema()
        {
            var Episode = new EnumType("Episode", new EnumValues
            {
                ["NEWHOPE"] = null,
                ["EMPIRE"] = null,
                ["JEDI"] = null
            });

            var EpisodeList = new List(Episode);

            // use NamedTypeReference as proxy to bypass circular dependencies
            var CharacterList = new List(new NamedTypeReference("Character"));
            var Character = new InterfaceType(
                "Character",
                fields: new Fields
                {
                    ["id"] = new Field(ScalarType.NonNullString),
                    ["name"] = new Field(ScalarType.NonNullString),
                    ["friends"] = new Field(CharacterList),
                    ["appearsIn"] = new Field(EpisodeList)
                }, meta: new Meta("Character in the movie"));


            var Human = new ObjectType(
                "Human",
                meta: new Meta("Human character"),
                implements: new[] {Character},
                fields: new Fields
                {
                    ["id"] = new Field(ScalarType.NonNullString),
                    ["name"] = new Field(ScalarType.String),
                    ["homePlanet"] = new Field(ScalarType.String),
                    ["friends"] = new Field(CharacterList),
                    ["appearsIn"] = new Field(EpisodeList)
                });

            var Query = new ObjectType(
                "Query",
                new Fields
                {
                    ["human"] = new Field(Human, new Args {["id"] = Argument.Arg(ScalarType.NonNullString)}),
                    ["character"] = new Field(Character, new Args {["id"] = Argument.Arg(ScalarType.NonNullString)}),
                    ["characters"] = new Field(CharacterList)
                });


            var HumanInput = new InputObjectType(
                "HumanInput",
                new InputFields()
                {
                    ["name"] = new InputObjectField(ScalarType.NonNullString)
                });

            var Mutation = new ObjectType(
                "Mutation",
                new Fields
                {
                    ["addHuman"] = new Field(Human, new Args {["human"] = Argument.Arg(HumanInput)})
                });


            var schema = new Schema(Query, Mutation, typesReferencedByNameOnly: new []
            {
                Character
            });
            return schema;
        }
    }
}