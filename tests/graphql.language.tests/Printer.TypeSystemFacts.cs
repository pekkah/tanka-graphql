using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests
{
    public class Printer_TypeSystemFacts
    {
        private void AssertPrintedEquals(string expected, string actual)
        {
            string Normalize(string str)
            {
                str = str
                    .Replace("\r", string.Empty)
                    .Replace("\n", string.Empty);

                return Regex.Replace(str, @"\s+", " ");
            }

            Assert.Equal(
                Normalize(expected),
                Normalize(actual),
                ignoreCase: false,
                ignoreLineEndingDifferences: true,
                ignoreWhiteSpaceDifferences: true
            );
        }

        [Fact]
        public void DirectiveDefinition()
        {
            /* Given */
            var source = "directive @name on QUERY";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseDirectiveDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void DirectiveDefinition_Arguments()
        {
            /* Given */
            var source = "directive @name(a: Int, b: Float) on QUERY";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseDirectiveDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void DirectiveDefinition_Description()
        {
            /* Given */
            var source = @"
""""""Description""""""
 directive @name repeatable on MUTATION";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseDirectiveDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void DirectiveDefinition_Location()
        {
            /* Given */
            var source = "directive @name on MUTATION";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseDirectiveDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void DirectiveDefinition_Repeatable()
        {
            /* Given */
            var source = "directive @name repeatable on MUTATION";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseDirectiveDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Theory]
        [InlineData("directive @name on FIELD")]
        [InlineData("directive @name on FIELD | QUERY")]
        [InlineData("directive @name on UNION | SCHEMA | FIELD")]
        public void DirectiveDefinition_DirectiveLocations(string source)
        {
            /* Given */
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseDirectiveDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void ScalarDefinition()
        {
            /* Given */
            var source = "scalar Name";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseScalarDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void ScalarDefinition_Description()
        {
            /* Given */
            var source = @"
""""""
Description
""""""
 scalar Name";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseScalarDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void ScalarDefinition_Directives()
        {
            /* Given */
            var source = "scalar Name @a @b";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseScalarDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void FieldDefinition()
        {
            /* Given */
            var source = "name: Int";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseFieldDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void FieldDefinition_Description()
        {
            /* Given */
            var source = @"
""""""Description""""""
 name: Int";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseFieldDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void FieldDefinition_Directives()
        {
            /* Given */
            var source = "name: Int @a @b @c";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseFieldDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void FieldDefinition_Arguments()
        {
            /* Given */
            var source = "name(a: Int!, b: custom): Int";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseFieldDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Theory]
        [InlineData("implements Name")]
        [InlineData("implements A & B")]
        public void ImplementsInterfaces(string source)
        {
            /* Given */
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseOptionalImplementsInterfaces()!);

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void ObjectDefinition()
        {
            /* Given */
            var source = "type Name";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseObjectDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void ObjectDefinition_Description()
        {
            /* Given */
            var source = @"
""""""Description""""""
 type Name";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseObjectDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void ObjectDefinition_Interfaces()
        {
            /* Given */
            var source = "type Name implements Interface";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseObjectDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void ObjectDefinition_Directives()
        {
            /* Given */
            var source = "type Name @a @b";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseObjectDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void ObjectDefinition_Fields()
        {
            /* Given */
            var source = @"
type Name {
    name: String
    version(includePreviews: Boolean): Float
 }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseObjectDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void ObjectDefinition_All()
        {
            /* Given */
            var source = @"
""""""
Description
""""""
 type Name implements A & B @a @b  {
    name: String
    version(includePreviews: Boolean): Float
 }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseObjectDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void InterfaceDefinition()
        {
            /* Given */
            var source = "interface Name";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseInterfaceDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void InterfaceDefinition_Description()
        {
            /* Given */
            var source = @"
""""""Description""""""
 interface Name";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseInterfaceDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void InterfaceDefinition_Interfaces()
        {
            /* Given */
            var source = "interface Name implements Interface";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseInterfaceDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void InterfaceDefinition_Directives()
        {
            /* Given */
            var source = "interface Name @a @b";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseInterfaceDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void InterfaceDefinition_Fields()
        {
            /* Given */
            var source = @"
interface Name {
    name: String
    version(includePreviews: Boolean): Float
 }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseInterfaceDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void InterfaceDefinition_All()
        {
            /* Given */
            var source = @"
""""""
Description
""""""
 interface Name implements A & B @a @b  {
    name: String
    version(includePreviews: Boolean): Float
 }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseInterfaceDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void UnionDefinition()
        {
            /* Given */
            var source = "union Name";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseUnionDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void UnionDefinition_Description()
        {
            /* Given */
            var source = @"
""""""Description""""""
 union Name";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseUnionDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void UnionDefinition_Members()
        {
            /* Given */
            var source = "union Name = A | B";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseUnionDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void UnionDefinition_Directives()
        {
            /* Given */
            var source = "union Name @a @b";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseUnionDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void UnionDefinition_All()
        {
            /* Given */
            var source = @"
""""""
Description
""""""
 union Name @a @b = A | B";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseUnionDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void EnumDefinition()
        {
            /* Given */
            var source = "enum Name";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseEnumDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void EnumDefinition_Description()
        {
            /* Given */
            var source = @"
""""""Description""""""
 enum Name";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseEnumDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void EnumDefinition_Values()
        {
            /* Given */
            var source = "enum Name { A B }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseEnumDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void EnumDefinition_Directives()
        {
            /* Given */
            var source = "enum Name @a @b";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseEnumDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void EnumDefinition_All()
        {
            /* Given */
            var source = @"
""""""
Description
""""""
 enum Name @a @b {
    A
    B
    C
 }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseEnumDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void InputObjectDefinition()
        {
            /* Given */
            var source = "input Name";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseInputObjectDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void InputObjectDefinition_Description()
        {
            /* Given */
            var source = @"
""""""Description""""""
 input Name";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseInputObjectDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void InputObjectDefinition_Directives()
        {
            /* Given */
            var source = "input Name @a @b";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseInputObjectDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void InputObjectDefinition_Fields()
        {
            /* Given */
            var source = @"
input Name {
    name: String
    version: Float
 }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseInputObjectDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void InputObjectDefinition_Fields2()
        {
            /* Given */
            var source = @"
input Name {
    name: String! = ""test""
    nullable: String
 }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseInputObjectDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void InputObjectDefinition_All()
        {
            /* Given */
            var source = @"
""""""
Description
""""""
 input Name @a @b  {
    name: String
    version: Float
 }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseInputObjectDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void SchemaDefinition()
        {
            /* Given */
            var source = "schema { query: TypeName }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseSchemaDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void SchemaDefinition_Description()
        {
            /* Given */
            var source = @"
""""""Description""""""
 schema { query: TypeName }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseSchemaDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void SchemaDefinition_Directives()
        {
            /* Given */
            var source = "schema @a @b { query: TypeName }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseSchemaDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void SchemaDefinition_AllRootTypes()
        {
            /* Given */
            var source = "schema { query: Query mutation: Mutation subscription: Subscription }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseSchemaDefinition());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void TypeSystemDocument_TypeDefinitions()
        {
            /* Given */
            var source = @"
""""""
Scalar
""""""
 scalar Scalar

""""""
Object
""""""
 type Object {
    field: Scalar!
 }
 
""""""
Interface
""""""
 interface Interface {
    """"""Field""""""
    field: Scalar!
 }
 
""""""
Union
""""""
 union Union = A | B
  
""""""
Enum
""""""
 enum Enum { 
    """"""A""""""
    A
    """"""B""""""
    B 
 }
 
""""""
Input
""""""
 input Input {
    """"""Field""""""
    field: Scalar
 }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseTypeSystemDocument());

            /* Then */
            AssertPrintedEquals(source, actual);
        }

        [Fact]
        public void TypeSystemDocument_TypeExtensions()
        {
            /* Given */
            var source = @"
extend scalar Scalar
 
  extend type Object {
    field: Scalar!
  }
 
  extend interface Interface {
    """"""Field""""""
    field: Scalar!
  }
 
  extend union Union = A | B
 
  extend enum Enum { 
    """"""A""""""
    A
    """"""B""""""
    B 
  }
 
  extend input Input {
    """"""Field""""""
    field: Scalar
  }";
            var sut = Parser.Create(source);

            /* When */
            var actual = Printer.Print(sut.ParseTypeSystemDocument());

            /* Then */
            AssertPrintedEquals(source, actual);
        }
    }
}