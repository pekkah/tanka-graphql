using System.Linq;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Xunit;

namespace Tanka.GraphQL.Tests.SDL
{
    public class SdlFacts
    {
        [Fact]
        public void Parse_custom_scalar()
        {
            /* Given */
            var idl = @"
                scalar Url

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var builder = new SchemaBuilder()
                .Sdl(idl);


            /* When */
            builder.TryGetType<ScalarType>("Url", out var actual);

            /* Then */
            Assert.Equal("Url", actual.Name);
        }

        [Fact]
        public void Parse_directives_on_input_object()
        {
            /* Given */
            var sdl = @"
                directive @map(from: String!, to: String!) on INPUT_OBJECT

                input Input @map(from: ""from"", to: ""to"") {
                }

                type Query {
                }
                
                schema {
                    query: Query
                }
                ";

            var schema = new SchemaBuilder()
                .Sdl(sdl)
                .Build();


            /* When */
            var directive = schema
                .GetNamedType<InputObjectType>("Input")
                .GetDirective("map");

            /* Then */
            Assert.NotNull(directive);
        }

        [Fact]
        public void Parse_directives_on_object()
        {
            /* Given */
            var sdl = @"
                directive @map(from: String!, to: String!) on OBJECT

                type Query @map(from: ""from"", to: ""to"") {
                }
                
                schema {
                    query: Query
                }
                ";

            var schema = new SchemaBuilder()
                .Sdl(sdl)
                .Build();


            /* When */
            var directive = schema
                .GetNamedType<ObjectType>("Query")
                .GetDirective("map");

            /* Then */
            Assert.NotNull(directive);
        }

        [Fact]
        public void Parse_directives_on_schema()
        {
            /* Given */
            var sdl = @"
                directive @map(from: String!, to: String!) on SCHEMA

                type Query {
                }

                
                schema @map(from: ""from"", to: ""to"") {
                    query: Query
                }
                ";

            var schema = new SchemaBuilder()
                .Sdl(sdl)
                .Build();


            /* When */
            var directive = schema.GetDirective("map");

            /* Then */
            Assert.NotNull(directive);
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

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);
            var reader = new SchemaReader(document);

            /* When */
            var schema = reader.Read().Build();
            var actual = schema.GetNamedType<ObjectType>("User");
            var fields = schema.GetFields(actual.Name);

            /* Then */
            Assert.NotNull(actual);
            Assert.Equal("User", actual.Name);
            Assert.Contains(fields, kv => kv.Key == "name" && (ScalarType) kv.Value.Type == ScalarType.String);
            Assert.Contains(fields,
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
                }";

            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var actual = new SchemaBuilder().Sdl(document)
                .Build();

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

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var actual = new SchemaBuilder().Sdl(document)
                .Build()
                .QueryTypes<INamedType>();

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

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var builder = new SchemaBuilder()
                .Sdl(idl);

            builder.TryGetType<ScalarType>("JediPowerLevel", out var jediPowerLevel);
            builder.TryGetType<ScalarType>("JediTrickLevel", out var jediTrickLevel);

            builder.Include("JediPowerLevel", new IntConverter())
                .Include("JediTrickLevel", new IntConverter());

            /* When */
            var schema = builder.Build();
            var actual = schema.QueryTypes<INamedType>();

            /* Then */
            Assert.Contains(actual, type => type.Name == "Episode" && type is EnumType);
            Assert.Contains(actual, type => type.Name == "Character" && type is InterfaceType);
            Assert.Contains(actual, type => type.Name == "Human" && type is ObjectType);
            Assert.Contains(actual, type => type.Name == "Droid" && type is ObjectType);
            Assert.Contains(actual, type => type.Name == "JediPowerInput" && type is InputObjectType);
            Assert.Contains(actual, type => type.Name == "JediPowerLevel" && type.GetType() == typeof(ScalarType));
            Assert.Contains(actual, type => type.Name == "JediTrickLevel" && type.GetType() == typeof(ScalarType));

            var jediPowerInput = (InputObjectType) actual.Single(t => t.Name == "JediPowerInput");
            var level = schema.GetInputField(jediPowerInput.Name, "level");
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

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);
            var reader = new SchemaReader(document);

            /* When */
            var actual = reader.Read().Build().GetNamedType<EnumType>("Episode");

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

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var actual = new SchemaBuilder().Sdl(document)
                .Build();

            /* Then */
            var character = actual.GetNamedType<InterfaceType>("Character");
            var human = actual.GetNamedType<ObjectType>("Human");
            var characterFields = actual.GetFields(character.Name);

            Assert.NotNull(character);
            Assert.NotNull(human);
            Assert.Equal("Character", character.Name);
            Assert.Contains(characterFields,
                field => field.Key == "parent" && (ObjectType) field.Value.Type == human);
        }

        [Fact]
        public void Parse_InterfaceType_with_self_reference()
        {
            /* Given */
            var idl = @"
                interface Character {
                    parent: Character
                }

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var schema = new SchemaBuilder().Sdl(document)
                .Build();

            /* Then */
            var character = schema.GetNamedType<InterfaceType>("Character");
            var characterFields = schema.GetFields(character.Name);

            Assert.NotNull(character);
            Assert.Equal("Character", character.Name);
            Assert.Contains(characterFields,
                field => field.Key == "parent" &&
                         (InterfaceType) field.Value.Type == character);
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

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var actual = new SchemaBuilder().Sdl(document)
                .Build();

            /* Then */
            var human = actual.GetNamedType<ObjectType>("Human");

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

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var actual = new SchemaBuilder().Sdl(document).Build();

            /* Then */
            var human = actual.GetNamedType<ObjectType>("Human");
            var humanFields = actual.GetFields(human.Name);

            Assert.NotNull(human);
            Assert.Equal("Human", human.Name);
            Assert.Single(humanFields, f => f.Key == "first");
            Assert.Single(humanFields, f => f.Key == "second");
        }


        [Fact]
        public void Parse_ObjectType_with_field_with_arguments()
        {
            /* Given */
            var idl = @"
                type User {
                    scopes(includeIdentity:Boolean!): [String!]!
                }

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var schema = new SchemaBuilder().Sdl(document).Build();
            var actual = schema.GetNamedType<ObjectType>("User");
            var actualFields = schema.GetFields("User");

            /* Then */
            Assert.Equal("User", actual.Name);

            var scopesField = actualFields.SingleOrDefault();
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

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var actual = new SchemaBuilder().Sdl(document).Build();

            /* Then */
            var character = actual.GetNamedType<InterfaceType>("Character");
            var human = actual.GetNamedType<ObjectType>("Human");
            var humanFields = actual.GetFields(human.Name);

            Assert.NotNull(character);
            Assert.NotNull(human);
            Assert.Equal("Human", human.Name);
            Assert.Contains(humanFields,
                field => field.Key == "parent" && (InterfaceType) field.Value.Type == character);
        }

        [Fact]
        public void Parse_ObjectType_with_self_reference()
        {
            /* Given */
            var idl = @"
                type Human {
                    parent: Human
                }

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var actual = new SchemaBuilder().Sdl(document).Build();

            /* Then */
            var human = actual.GetNamedType<ObjectType>("Human");
            var humanFields = actual.GetFields(human.Name);

            Assert.NotNull(human);
            Assert.Equal("Human", human.Name);
            Assert.Contains(humanFields,
                field => field.Key == "parent" &&
                         (ObjectType) field.Value.Type == human);
        }

        [Fact]
        public void Parse_simple_InterfaceType()
        {
            /* Given */
            var idl = @"
                interface Person {
                    name: String
                }

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var schema = new SchemaBuilder().Sdl(document).Build();
            var actual = schema.GetNamedType<InterfaceType>("Person");
            var actualFields = schema.GetFields(actual.Name);

            /* Then */
            Assert.Equal("Person", actual.Name);
            Assert.Contains(actualFields, kv => kv.Key == "name" && (ScalarType) kv.Value.Type == ScalarType.String);
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

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var schema = new SchemaBuilder().Sdl(document).Build();
            var actual = schema.GetNamedType<ObjectType>("User");
            var actualFields = schema.GetFields(actual.Name);

            /* Then */
            Assert.Equal("User", actual.Name);
            Assert.Contains(actualFields, kv => kv.Key == "name" && (ScalarType) kv.Value.Type == ScalarType.String);
            Assert.Contains(actualFields,
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

                type Query {
                }
                schema {
                    query: Query
                }
                ";
            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var schema = new SchemaBuilder().Sdl(document).Build();
            var actual = schema.GetNamedType<ObjectType>("User");
            var actualFields = schema.GetFields(actual.Name);

            /* Then */
            Assert.Equal("User", actual.Name);
            Assert.Contains(actualFields,
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

                type Query {
                }
                schema {
                    query: Query
                }
                ";

            var document = Parser.ParseTypeSystemDocument(idl);

            /* When */
            var schema = new SchemaBuilder().Sdl(document).Build();
            var actual = schema.GetNamedType<ObjectType>("User");
            var actualFields = schema.GetFields(actual.Name);

            /* Then */
            Assert.Equal("User", actual.Name);
            Assert.Contains(actualFields,
                kv => kv.Key == "name" && (NonNull) kv.Value.Type == new NonNull(ScalarType.String));
        }
    }
}