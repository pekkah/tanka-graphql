using fugu.graphql.type;
using static fugu.graphql.type.Argument;
using static fugu.graphql.type.ScalarType;

namespace fugu.graphql.tests.data.validation
{
    public class WeirdSchema : Schema
    {
        public WeirdSchema()
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

            Query = new ObjectType(
                "Query",
                new Fields
                {
                    ["dog"] = new Field(Dog),
                    ["catOrDog"] = new Field(CatOrDog),
                });

            Mutation = new ObjectType(
                "Mutation",
                new Fields()
                {
                    ["mutateDog"] = new Field(Dog)
                });
        }

        public UnionType HumanOrAlien { get; }

        public UnionType DogOrHuman { get; }

        public UnionType CatOrDog { get; }

        public ObjectType Cat { get; }

        public EnumType CatCommand { get; }

        public ObjectType Alien { get; }

        public ObjectType Dog { get; }

        public InterfaceType Sentient { get; }

        public ObjectType Human { get; }

        public InterfaceType Pet { get; }

        public EnumType DogCommand { get; }
    }
}