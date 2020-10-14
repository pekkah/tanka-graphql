using Tanka.GraphQL.Language;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.TypeSystem;
using Xunit;


namespace Tanka.GraphQL.Tests.TypeSystem
{
    public class SdlPrinterFacts
    {
        [Fact]
        public void ScalarType()
        {
            /* Given */
            var source = @"
""""""description""""""
scalar Custom";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }

        [Theory]
        [InlineData("Boolean", "true")]
        [InlineData("Float", "123.123")]
        [InlineData("ID", "\"111\"")]
        [InlineData("Int", "123")]
        [InlineData("String", "\"some text here.\"")]
        public void Scalar_Standard_AsDefaultValue(string scalarType, string defaultValue)
        {
            /* Given */
            var source = $"directive @ignore(arg1: {scalarType} = {defaultValue}) on SCALAR";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }

        [Fact]
        public void DirectiveDefinition()
        {
            /* Given */
            var source = @"
""""""description""""""
directive @custom(arg1: Int = 123) on SCALAR";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }

        [Fact]
        public void InputObjectType()
        {
            /* Given */
            var source = @"
""""""description""""""
input Custom {
    field1: Int!
    field2: String! = ""test""
 }";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }

        [Fact]
        public void EnumType()
        {
            /* Given */
            var source = @"
""""""description""""""
enum Custom {
    ONE
    TWO
 }";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }

        [Fact]
        public void InterfaceType()
        {
            /* Given */
            var source = @"
""""""description""""""
interface Custom {
    field1: String!
    field2: Int
    field3: [Int!]!
    field4(a: Int, b: Float): Float!
}";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }

        [Fact(Skip = "NotSupported")]
        public void InterfaceType_Implements()
        {
            /* Given */
            var source = @"
interface Implemented {
    field1: String!
}

""""""description""""""
interface Custom implements Implemented {
    field1: String!
}";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }

        [Fact]
        public void ObjectType()
        {
            /* Given */
            var source = @"
""""""description""""""
type Custom {
    field1: String!
    field2: Int
    field3: [Int!]!
    field4(a: Int, b: Float): Float!
}";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }

        [Fact]
        public void ObjectType_Implements()
        {
            /* Given */
            var source = @"
interface Implemented {
    field1: String!
}

""""""description""""""
type Custom implements Implemented {
    field1: String!
}";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }

        [Fact]
        public void UnionType()
        {
            /* Given */
            var source = @"
type A
type B

""""""description""""""
union Custom = A | B";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }

        [Fact]
        public void Query()
        {
            /* Given */
            var source = @"
type Query

schema {
    query: Query
}";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }

        [Fact]
        public void Query_and_mutation()
        {
            /* Given */
            var source = @"
type Query
type Mutation

schema {
    query: Query
    mutation: Mutation
}";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }

        [Fact]
        public void Query_and_mutation_and_subscription()
        {
            /* Given */
            var source = @"
type Query
type Mutation
type Subscription

schema {
    query: Query
    mutation: Mutation
    subscription: Subscription
}";
            var schema = new SchemaBuilder()
                .Sdl(source)
                .Build(false);

            /* When */
            var actual = SdlPrinter
                .Print(new SdlPrinterOptions(schema));

            /* Then */
            Gql.AssertEqual(source, Printer.Print(actual));
        }
    }
}