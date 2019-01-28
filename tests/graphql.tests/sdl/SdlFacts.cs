using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.sdl;
using tanka.graphql.type;
using tanka.graphql.type.converters;
using Xunit;

namespace tanka.graphql.tests.sdl
{
    public class SdlFacts
    {
        [Fact]
        public void Parse_custom_scalar()
        {
            /* Given */
            var idl = @"
scalar Url
";
            var urlScalar = new ScalarType("Url", new StringConverter());
            var document = Parser.ParseDocument(idl);
            var typeDefinition = document.Definitions.OfType<GraphQLScalarTypeDefinition>().SingleOrDefault();
            var context = new SdlParserContext(document, new INamedType[]
            {
                urlScalar
            });

            /* When */
            var actual = Sdl.Scalar(typeDefinition, context);

            /* Then */
            Assert.Equal("Url", actual.Name);
            Assert.Equal(urlScalar, actual);
        }

        [Fact]
        public void Parse_Document()
        {
            /* Given */
            var idl = @"
type User {
    name: String
    password: String
}
";
            var document = Parser.ParseDocument(idl);
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Document(document, context).OfType<ObjectType>().SingleOrDefault();

            /* Then */
            Assert.NotNull(actual);
            Assert.Equal("User", actual.Name);
            Assert.Contains(actual.Fields, kv => kv.Key == "name" && (ScalarType) kv.Value.Type == ScalarType.String);
            Assert.Contains(actual.Fields,
                kv => kv.Key == "password" && (ScalarType) kv.Value.Type == ScalarType.String);
        }

        [Fact]
        public void Parse_Document_as_Schema()
        {
            /* Given */
            var idl = @"
type Query {
}

type Mutation {
}

type Subscription {

}

schema {
  query: Query
  mutation: Mutation
  subscription: Subscription
}
";
            var document = Parser.ParseDocument(idl);

            /* When */
            var actual = Sdl.Schema(document);

            /* Then */
            Assert.NotNull(actual);
            Assert.NotNull(actual.Query);
            Assert.NotNull(actual.Mutation);
            Assert.NotNull(actual.Subscription);
        }

        [Fact]
        public void Parse_Document_with_two_types()
        {
            /* Given */
            var idl = @"
type User {
    name: String
    password: String
}

type Role {
    name: String
    id: Int
}
";
            var document = Parser.ParseDocument(idl);
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Document(document, context).ToList();

            /* Then */
            Assert.Contains(actual, user => user.Name == "User");
            Assert.Contains(actual, user => user.Name == "Role");
        }

        [Fact]
        public void Parse_Document_with_types()
        {
            /* Given */
            var idl = @"
scalar JediPowerLevel
scalar JediTrickLevel

enum Episode { NEWHOPE, EMPIRE, JEDI }

interface Character {
  id: String!
  name: String
  friends: [Character]
  appearsIn: [Episode]
}

type Human implements Character {
  id: String!
  name: String
  friends: [Character]
  appearsIn: [Episode]
  homePlanet: String
}

type Droid implements Character {
  id: String!
  name: String
  friends: [Character]
  appearsIn: [Episode]
  primaryFunction: String
}

input JediPowerInput {
    power: String
    level: JediPowerLevel
}
";
            var jediPowerLevel = new ScalarType("JediPowerLevel", new LongConverter());
            var document = Parser.ParseDocument(idl);
            var context = new SdlParserContext(document,
                new[] {jediPowerLevel, new ScalarType("JediTrickLevel", new DoubleConverter())});

            /* When */
            var actual = Sdl.Document(document, context).ToList();

            /* Then */
            Assert.Contains(actual, type => type.Name == "Episode" && type is EnumType);
            Assert.Contains(actual, type => type.Name == "Character" && type is InterfaceType);
            Assert.Contains(actual, type => type.Name == "Human" && type is ObjectType);
            Assert.Contains(actual, type => type.Name == "Droid" && type is ObjectType);
            Assert.Contains(actual, type => type.Name == "JediPowerInput" && type is InputObjectType);
            Assert.Contains(actual, type => type.Name == "JediPowerLevel" && type.GetType() == typeof(ScalarType));
            Assert.Contains(actual, type => type.Name == "JediTrickLevel" && type.GetType() == typeof(ScalarType));

            var jediPowerInput = (InputObjectType) actual.Single(t => t.Name == "JediPowerInput");
            var level = jediPowerInput.GetField("level");
            Assert.Equal(jediPowerLevel, level.Type);
        }


        [Fact]
        public void Parse_EnumType()
        {
            /* Given */
            var idl = @"
enum Episode {
  NEWHOPE
  EMPIRE
  JEDI
}
";
            var document = Parser.ParseDocument(idl);
            var typeDefinition = document.Definitions.OfType<GraphQLEnumTypeDefinition>().SingleOrDefault();
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Enum(typeDefinition, context);

            /* Then */
            Assert.Equal("Episode", actual.Name);
            Assert.Contains(actual.Values, kv => kv.Key == "NEWHOPE");
            Assert.Contains(actual.Values, kv => kv.Key == "EMPIRE");
            Assert.Contains(actual.Values, kv => kv.Key == "JEDI");
        }

        [Fact]
        public void Parse_InterfaceType_with_object_field()
        {
            /* Given */
            var idl = @"
interface Character {
    parent: Human
}

type Human implements Character { 
}
";
            var document = Parser.ParseDocument(idl);
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Document(document, context).ToList();

            /* Then */
            var character = actual.OfType<InterfaceType>().SingleOrDefault();
            var human = actual.OfType<ObjectType>().SingleOrDefault();

            Assert.NotNull(character);
            Assert.NotNull(human);
            Assert.Equal("Character", character.Name);
            Assert.Contains(character.Fields,
                field => field.Key == "parent" && ((NamedTypeReference) field.Value.Type).TypeName == human.Name);
        }

        [Fact]
        public void Parse_InterfaceType_with_self_reference()
        {
            /* Given */
            var idl = @"
interface Character {
    parent: Character
}
";
            var document = Parser.ParseDocument(idl);
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Document(document, context);

            /* Then */
            var character = actual.OfType<InterfaceType>().SingleOrDefault();

            Assert.NotNull(character);
            Assert.Equal("Character", character.Name);
            Assert.Contains(character.Fields,
                field => field.Key == "parent" &&
                         (NamedTypeReference) field.Value.Type == new NamedTypeReference("Character"));
        }

        [Fact]
        public void Parse_ObjectType_implementing_interface()
        {
            /* Given */
            var idl = @"
interface Character {

}

type Human implements Character {

}
";
            var document = Parser.ParseDocument(idl);
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Document(document, context);

            /* Then */
            var human = actual.OfType<ObjectType>().SingleOrDefault();

            Assert.NotNull(human);
            Assert.Equal("Human", human.Name);
            Assert.Contains(human.Interfaces, type => type.Name == "Character");
        }

        [Fact]
        public void Parse_ObjectType_with_extension()
        {
            /* Given */
            var idl = @"
extend type Human {
    second: Boolean
}

type Human {
    first: Int
}
";
            var document = Parser.ParseDocument(idl);
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Document(document, context);

            /* Then */
            var human = actual.OfType<ObjectType>().SingleOrDefault();

            Assert.NotNull(human);
            Assert.Equal("Human", human.Name);
            Assert.Single(human.Fields, f => f.Key == "first");
            Assert.Single(human.Fields, f => f.Key == "second");
        }


        [Fact]
        public void Parse_ObjectType_with_field_with_arguments()
        {
            /* Given */
            var idl = @"
type User {
    scopes(includeIdentity:Boolean!): [String!]!
}
";
            var document = Parser.ParseDocument(idl);
            var objectTypeDefinition = document.Definitions.OfType<GraphQLObjectTypeDefinition>().SingleOrDefault();
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Object(objectTypeDefinition, context);

            /* Then */
            Assert.Equal("User", actual.Name);

            var scopesField = actual.Fields.SingleOrDefault();
            Assert.Equal("scopes", scopesField.Key);
            Assert.Contains(scopesField.Value.Arguments,
                a => a.Key == "includeIdentity" && (ScalarType) a.Value.Type.Unwrap() == ScalarType.Boolean);
        }

        [Fact]
        public void Parse_ObjectType_with_inteface_field()
        {
            /* Given */
            var idl = @"
interface Character {

}

type Human implements Character {
    parent: Character
}
";
            var document = Parser.ParseDocument(idl);
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Document(document, context).ToList();

            /* Then */
            var character = actual.OfType<InterfaceType>().SingleOrDefault();
            var human = actual.OfType<ObjectType>().SingleOrDefault();

            Assert.NotNull(character);
            Assert.NotNull(human);
            Assert.Equal("Human", human.Name);
            Assert.Contains(human.Fields,
                field => field.Key == "parent" && (InterfaceType) field.Value.Type == character);
        }

        [Fact]
        public void Parse_ObjectType_with_self_reference()
        {
            /* Given */
            var idl = @"
type Human implements Character {
    parent: Human
}
";
            var document = Parser.ParseDocument(idl);
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Document(document, context);

            /* Then */
            var human = actual.OfType<ObjectType>().SingleOrDefault();

            Assert.NotNull(human);
            Assert.Equal("Human", human.Name);
            Assert.Contains(human.Fields,
                field => field.Key == "parent" &&
                         (NamedTypeReference) field.Value.Type == new NamedTypeReference("Human"));
        }

        [Fact]
        public void Parse_simple_InterfaceType()
        {
            /* Given */
            var idl = @"
interface Person {
    name: String
}
";
            var document = Parser.ParseDocument(idl);
            var typeDefinition = document.Definitions.OfType<GraphQLInterfaceTypeDefinition>().SingleOrDefault();
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Interface(typeDefinition, context);

            /* Then */
            Assert.Equal("Person", actual.Name);
            Assert.Contains(actual.Fields, kv => kv.Key == "name" && (ScalarType) kv.Value.Type == ScalarType.String);
        }

        [Fact]
        public void Parse_simple_ObjectType()
        {
            /* Given */
            var idl = @"
type User {
    name: String
    password: String
}
";
            var document = Parser.ParseDocument(idl);
            var objectTypeDefinition = document.Definitions.OfType<GraphQLObjectTypeDefinition>().SingleOrDefault();
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Object(objectTypeDefinition, context);

            /* Then */
            Assert.Equal("User", actual.Name);
            Assert.Contains(actual.Fields, kv => kv.Key == "name" && (ScalarType) kv.Value.Type == ScalarType.String);
            Assert.Contains(actual.Fields,
                kv => kv.Key == "password" && (ScalarType) kv.Value.Type == ScalarType.String);
        }

        [Fact]
        public void Parse_simple_ObjectType_with_list()
        {
            /* Given */
            var idl = @"
type User {
    names: [String]
}
";
            var document = Parser.ParseDocument(idl);
            var objectTypeDefinition = document.Definitions.OfType<GraphQLObjectTypeDefinition>().SingleOrDefault();
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Object(objectTypeDefinition, context);

            /* Then */
            Assert.Equal("User", actual.Name);
            Assert.Contains(actual.Fields,
                kv => kv.Key == "names" && (List) kv.Value.Type == new List(ScalarType.String));
        }

        [Fact]
        public void Parse_simple_ObjectType_with_non_null()
        {
            /* Given */
            var idl = @"
type User {
    name: String!
}
";
            var document = Parser.ParseDocument(idl);
            var objectTypeDefinition = document.Definitions.OfType<GraphQLObjectTypeDefinition>().SingleOrDefault();
            var context = new SdlParserContext(document);

            /* When */
            var actual = Sdl.Object(objectTypeDefinition, context);

            /* Then */
            Assert.Equal("User", actual.Name);
            Assert.Contains(actual.Fields,
                kv => kv.Key == "name" && (NonNull) kv.Value.Type == new NonNull(ScalarType.String));
        }
    }
}