using fugu.graphql.type;
using static fugu.graphql.type.Argument;
using static fugu.graphql.type.ScalarType;

namespace fugu.graphql.tests.data.validation
{
    public class WeirdSchemaBuilder
    {
        static WeirdSchemaBuilder()
        {
            Pet = new InterfaceType(
                "Pet",
                new Fields
                {
                    ["name"] = new Field(NonNullString)
                });

            Sentient = new InterfaceType(
                "Sentient",
                new Fields
                {
                    ["name"] = new Field(NonNullString)
                });

            Human = new ObjectType(
                "Human",
                implements: new[] {Sentient},
                fields: new Fields
                {
                    ["name"] = new Field(NonNullString)
                });

            Alien = new ObjectType(
                "Alient",
                implements: new[] {Sentient},
                fields: new Fields
                {
                    ["name"] = new Field(NonNullString),
                    ["homePlanet"] = new Field(String)
                });

            DogCommand = new EnumType(
                "DogCommand",
                new EnumValues
                {
                    ["SIT"] = null,
                    ["DOWN"] = null,
                    ["HEEL"] = null
                });

            Dog = new ObjectType(
                "Dog",
                implements: new[] {Pet},
                fields: new Fields
                {
                    ["name"] = new Field(NonNullString),
                    ["nickname"] = new Field(String),
                    ["backVolume"] = new Field(Int),
                    ["doesKnowCommand"] = new Field(NonNullBoolean,
                        new Args
                        {
                            ["dogCommand"] = Arg(new NonNull(DogCommand))
                        }),
                    ["isHousetrained"] = new Field(NonNullBoolean,
                        new Args
                        {
                            ["atOtherHomes"] = Arg(Boolean)
                        }),
                    ["owner"] = new Field(Human)
                });

            CatCommand = new EnumType(
                "CatCommand",
                new EnumValues
                {
                    ["JUMP"] = null
                });

            Cat = new ObjectType(
                "Cat",
                implements: new[] {Pet},
                fields: new Fields
                {
                    ["name"] = new Field(NonNullString),
                    ["nickname"] = new Field(String),
                    ["doesKnowCommand"] = new Field(NonNullBoolean,
                        new Args
                        {
                            ["catCommand"] = Arg(new NonNull(CatCommand))
                        }),
                    ["meowVolume"] = new Field(Int)
                });

            CatOrDog = new UnionType(
                "CatOrDog",
                new[] {Cat, Dog});

            DogOrHuman = new UnionType(
                "DogOrHuman",
                new[] {Dog, Human});

            HumanOrAlien = new UnionType(
                "HumanOrAlien",
                new[] {Human, Alien});
        }

        public static ISchema Build()
        {
            var query = new ObjectType(
                "Query",
                new Fields
                {
                    ["dog"] = new Field(Dog),
                    ["catOrDog"] = new Field(CatOrDog),
                });

            var mutation = new ObjectType(
                "Mutation",
                new Fields()
                {
                    ["mutateDog"] = new Field(Dog)
                });

            return new Schema(query, mutation);
        }

        public static UnionType HumanOrAlien { get; }

        public static UnionType DogOrHuman { get; }

        public static UnionType CatOrDog { get; }

        public static ObjectType Cat { get; }

        public static EnumType CatCommand { get; }

        public static ObjectType Alien { get; }

        public static ObjectType Dog { get; }

        public static InterfaceType Sentient { get; }

        public static ObjectType Human { get; }

        public static InterfaceType Pet { get; }

        public static EnumType DogCommand { get; }
    }
}