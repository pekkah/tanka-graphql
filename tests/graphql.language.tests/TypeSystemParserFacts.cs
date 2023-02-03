using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests;

public class TypeSystemParserFacts
{
    [Fact]
    public void DirectiveDefinition()
    {
        /* Given */
        var sut = Parser.Create("directive @name on QUERY");

        /* When */
        var definition = sut.ParseDirectiveDefinition();

        /* Then */
        Assert.Equal("name", definition.Name);
        Assert.False(definition.IsRepeatable);
        Assert.Null(definition.Description);
    }

    [Fact]
    public void DirectiveDefinition_Arguments()
    {
        /* Given */
        var sut = Parser.Create("directive @name(a:Int, b:Float) on QUERY");

        /* When */
        var definition = sut.ParseDirectiveDefinition();

        /* Then */
        Assert.Equal(2, definition.Arguments?.Count);
    }

    [Fact]
    public void DirectiveDefinition_Description()
    {
        /* Given */
        var sut = Parser.Create(@"
""Description""
directive @name repeatable on MUTATION");

        /* When */
        var definition = sut.ParseDirectiveDefinition();

        /* Then */
        Assert.Equal("Description", definition.Description);
    }

    [Fact]
    public void DirectiveDefinition_Location()
    {
        /* Given */
        var sut = Parser.Create("directive @name on MUTATION");

        /* When */
        var definition = sut.ParseDirectiveDefinition();

        /* Then */
        Assert.Single(definition.DirectiveLocations, ExecutableDirectiveLocations.MUTATION);
    }

    [Fact]
    public void DirectiveDefinition_Repeatable()
    {
        /* Given */
        var sut = Parser.Create("directive @name repeatable on MUTATION");

        /* When */
        var definition = sut.ParseDirectiveDefinition();

        /* Then */
        Assert.True(definition.IsRepeatable);
    }

    [Fact]
    public void DirectiveDefinition_Repeatable2()
    {
        /* Given */
        var sut = Parser.Create("directive @key(fields: _FieldSet!) repeatable on OBJECT | INTERFACE");

        /* When */
        var definition = sut.ParseDirectiveDefinition();

        /* Then */
        Assert.True(definition.IsRepeatable);
    }

    [Theory]
    [InlineData("on FIELD", ExecutableDirectiveLocations.FIELD)]
    [InlineData("on | FIELD", ExecutableDirectiveLocations.FIELD)]
    [InlineData("on FIELD | QUERY", ExecutableDirectiveLocations.FIELD)]
    [InlineData("on UNION | SCHEMA | FIELD", ExecutableDirectiveLocations.FIELD)]
    public void DirectiveLocations(string source, string expectedLocation)
    {
        /* Given */
        var sut = Parser.Create(source);

        /* When */
        var locations = sut.ParseDirectiveLocations();

        /* Then */
        Assert.Single(locations, expectedLocation);
    }

    [Fact]
    public void ScalarDefinition()
    {
        /* Given */
        var sut = Parser.Create("scalar Name");

        /* When */
        var definition = sut.ParseScalarDefinition();

        /* Then */
        Assert.Null(definition.Description);
        Assert.Equal("Name", definition.Name);
        Assert.Null(definition.Directives);
    }

    [Fact]
    public void ScalarDefinition_Description()
    {
        /* Given */
        var sut = Parser.Create(@"
""""""
Description
""""""
scalar Name");

        /* When */
        var definition = sut.ParseScalarDefinition();

        /* Then */
        Assert.Equal("Description", definition.Description);
    }

    [Fact]
    public void ScalarDefinition_Directives()
    {
        /* Given */
        var sut = Parser.Create("scalar Name @a @b");

        /* When */
        var definition = sut.ParseScalarDefinition();

        /* Then */
        Assert.Equal(2, definition.Directives?.Count);
    }

    [Fact]
    public void FieldDefinition()
    {
        /* Given */
        var sut = Parser.Create("name: Int");

        /* When */
        var definition = sut.ParseFieldDefinition();

        /* Then */
        Assert.Null(definition.Description);
        Assert.Equal("name", definition.Name);
        var namedType = Assert.IsType<NamedType>(definition.Type);
        Assert.Equal("Int", namedType.Name);
        Assert.Null(definition.Directives);
    }

    [Fact]
    public void FieldDefinition_Description()
    {
        /* Given */
        var sut = Parser.Create(@"
""Description""
name: Int");

        /* When */
        var definition = sut.ParseFieldDefinition();

        /* Then */
        Assert.Equal("Description", definition.Description);
    }

    [Fact]
    public void FieldDefinition_Directives()
    {
        /* Given */
        var sut = Parser.Create("name: Int @a @b @c");

        /* When */
        var definition = sut.ParseFieldDefinition();

        /* Then */
        Assert.Equal(3, definition.Directives?.Count);
    }

    [Fact]
    public void FieldDefinition_Arguments()
    {
        /* Given */
        var sut = Parser.Create("name(a: Int!, b: custom):Int");

        /* When */
        var definition = sut.ParseFieldDefinition();

        /* Then */
        Assert.Equal(2, definition.Arguments?.Count);
    }

    [Theory]
    [InlineData("implements Name", "Name")]
    [InlineData("implements & Name", "Name")]
    [InlineData("implements A & B", "A")]
    [InlineData("implements A & B", "B")]
    public void ImplementsInterfaces(string source, string expectedInterface)
    {
        /* Given */
        var sut = Parser.Create(source);

        /* When */
        var interfaces = sut.ParseOptionalImplementsInterfaces();

        /* Then */
        Assert.NotNull(interfaces);
        Assert.Single(interfaces, i => (string)i.Name == expectedInterface);
    }

    [Fact]
    public void ObjectDefinition()
    {
        /* Given */
        var sut = Parser.Create("type Name");

        /* When */
        var definition = sut.ParseObjectDefinition();

        /* Then */
        Assert.Null(definition.Description);
        Assert.Equal("Name", definition.Name);
        Assert.Null(definition.Interfaces);
        Assert.Null(definition.Directives);
        Assert.Null(definition.Fields);
    }

    [Fact]
    public void ObjectDefinition_Description()
    {
        /* Given */
        var sut = Parser.Create(@"
""Description""
type Name");

        /* When */
        var definition = sut.ParseObjectDefinition();

        /* Then */
        Assert.Equal("Description", definition.Description);
    }

    [Fact]
    public void ObjectDefinition_Interfaces()
    {
        /* Given */
        var sut = Parser.Create("type Name implements Interface");

        /* When */
        var definition = sut.ParseObjectDefinition();

        /* Then */
        Assert.Equal(1, definition.Interfaces?.Count);
    }

    [Fact]
    public void ObjectDefinition_Directives()
    {
        /* Given */
        var sut = Parser.Create("type Name @a @b");

        /* When */
        var definition = sut.ParseObjectDefinition();

        /* Then */
        Assert.Equal(2, definition.Directives?.Count);
    }

    [Fact]
    public void ObjectDefinition_Fields()
    {
        /* Given */
        var sut = Parser.Create(@"
type Name {
    name: String
    version(includePreviews: Boolean): Float
}");

        /* When */
        var definition = sut.ParseObjectDefinition();

        /* Then */
        Assert.Equal(2, definition.Fields?.Count);
    }

    [Fact]
    public void ObjectDefinition_All()
    {
        /* Given */
        var sut = Parser.Create(@"
""""""
  Description
""""""
type Name implements A & B @a @b  {
    name: String
    version(includePreviews: Boolean): Float
}");

        /* When */
        var definition = sut.ParseObjectDefinition();

        /* Then */
        Assert.NotNull(definition.Description);
        Assert.Equal("Name", definition.Name);
        Assert.NotNull(definition.Interfaces);
        Assert.NotNull(definition.Directives);
        Assert.NotNull(definition.Fields);
    }

    [Fact]
    public void InterfaceDefinition()
    {
        /* Given */
        var sut = Parser.Create("interface Name");

        /* When */
        var definition = sut.ParseInterfaceDefinition();

        /* Then */
        Assert.Null(definition.Description);
        Assert.Equal("Name", definition.Name);
        Assert.Null(definition.Interfaces);
        Assert.Null(definition.Directives);
        Assert.Null(definition.Fields);
    }

    [Fact]
    public void InterfaceDefinition_Description()
    {
        /* Given */
        var sut = Parser.Create(@"
""Description""
interface Name");

        /* When */
        var definition = sut.ParseInterfaceDefinition();

        /* Then */
        Assert.Equal("Description", definition.Description);
    }

    [Fact]
    public void InterfaceDefinition_Interfaces()
    {
        /* Given */
        var sut = Parser.Create("interface Name implements Interface");

        /* When */
        var definition = sut.ParseInterfaceDefinition();

        /* Then */
        Assert.Equal(1, definition.Interfaces?.Count);
    }

    [Fact]
    public void InterfaceDefinition_Directives()
    {
        /* Given */
        var sut = Parser.Create("interface Name @a @b");

        /* When */
        var definition = sut.ParseInterfaceDefinition();

        /* Then */
        Assert.Equal(2, definition.Directives?.Count);
    }

    [Fact]
    public void InterfaceDefinition_Fields()
    {
        /* Given */
        var sut = Parser.Create(@"
interface Name {
    name: String
    version(includePreviews: Boolean): Float
}");

        /* When */
        var definition = sut.ParseInterfaceDefinition();

        /* Then */
        Assert.Equal(2, definition.Fields?.Count);
    }

    [Fact]
    public void InterfaceDefinition_All()
    {
        /* Given */
        var sut = Parser.Create(@"
""""""
  Description
""""""
interface Name implements A & B @a @b  {
    name: String
    version(includePreviews: Boolean): Float
}");

        /* When */
        var definition = sut.ParseInterfaceDefinition();

        /* Then */
        Assert.NotNull(definition.Description);
        Assert.Equal("Name", definition.Name);
        Assert.NotNull(definition.Interfaces);
        Assert.NotNull(definition.Directives);
        Assert.NotNull(definition.Fields);
    }

    [Fact]
    public void UnionDefinition()
    {
        /* Given */
        var sut = Parser.Create("union Name");

        /* When */
        var definition = sut.ParseUnionDefinition();

        /* Then */
        Assert.Null(definition.Description);
        Assert.Equal("Name", definition.Name);
        Assert.Null(definition.Members);
        Assert.Null(definition.Directives);
    }

    [Fact]
    public void UnionDefinition_Description()
    {
        /* Given */
        var sut = Parser.Create(@"
""Description""
union Name");

        /* When */
        var definition = sut.ParseUnionDefinition();

        /* Then */
        Assert.Equal("Description", definition.Description);
    }

    [Fact]
    public void UnionDefinition_Members()
    {
        /* Given */
        var sut = Parser.Create("union Name = A | B");

        /* When */
        var definition = sut.ParseUnionDefinition();

        /* Then */
        Assert.Equal(2, definition.Members?.Count);
    }

    [Fact]
    public void UnionDefinition_Directives()
    {
        /* Given */
        var sut = Parser.Create("union Name @a @b");

        /* When */
        var definition = sut.ParseUnionDefinition();

        /* Then */
        Assert.Equal(2, definition.Directives?.Count);
    }

    [Fact]
    public void UnionDefinition_All()
    {
        /* Given */
        var sut = Parser.Create(@"
""""""
  Description
""""""
union Name @a @b = A | B");

        /* When */
        var definition = sut.ParseUnionDefinition();

        /* Then */
        Assert.NotNull(definition.Description);
        Assert.Equal("Name", definition.Name);
        Assert.NotNull(definition.Members);
        Assert.NotNull(definition.Directives);
    }

    [Fact]
    public void EnumDefinition()
    {
        /* Given */
        var sut = Parser.Create("enum Name");

        /* When */
        var definition = sut.ParseEnumDefinition();

        /* Then */
        Assert.Null(definition.Description);
        Assert.Equal("Name", definition.Name);
        Assert.Null(definition.Values);
        Assert.Null(definition.Directives);
    }

    [Fact]
    public void EnumDefinition_Description()
    {
        /* Given */
        var sut = Parser.Create(@"
""Description""
enum Name");

        /* When */
        var definition = sut.ParseEnumDefinition();

        /* Then */
        Assert.Equal("Description", definition.Description);
    }

    [Fact]
    public void EnumDefinition_Values()
    {
        /* Given */
        var sut = Parser.Create("enum Name { A B }");

        /* When */
        var definition = sut.ParseEnumDefinition();

        /* Then */
        Assert.Equal(2, definition.Values?.Count);
    }

    [Fact]
    public void EnumDefinition_Directives()
    {
        /* Given */
        var sut = Parser.Create("enum Name @a @b");

        /* When */
        var definition = sut.ParseEnumDefinition();

        /* Then */
        Assert.Equal(2, definition.Directives?.Count);
    }

    [Fact]
    public void InputObjectDefinition()
    {
        /* Given */
        var sut = Parser.Create("input Name");

        /* When */
        var definition = sut.ParseInputObjectDefinition();

        /* Then */
        Assert.Null(definition.Description);
        Assert.Equal("Name", definition.Name);
        Assert.Null(definition.Directives);
        Assert.Null(definition.Fields);
    }

    [Fact]
    public void InputObjectDefinition_Description()
    {
        /* Given */
        var sut = Parser.Create(@"
""Description""
input Name");

        /* When */
        var definition = sut.ParseInputObjectDefinition();

        /* Then */
        Assert.Equal("Description", definition.Description);
    }

    [Fact]
    public void InputObjectDefinition_Directives()
    {
        /* Given */
        var sut = Parser.Create("input Name @a @b");

        /* When */
        var definition = sut.ParseInputObjectDefinition();

        /* Then */
        Assert.Equal(2, definition.Directives?.Count);
    }

    [Fact]
    public void InputObjectDefinition_Fields()
    {
        /* Given */
        var sut = Parser.Create(@"
input Name {
    name: String
    version: Float
}");

        /* When */
        var definition = sut.ParseInputObjectDefinition();

        /* Then */
        Assert.Equal(2, definition.Fields?.Count);
    }

    [Fact]
    public void InputObjectDefinition_All()
    {
        /* Given */
        var sut = Parser.Create(@"
""""""
  Description
""""""
input Name @a @b  {
    name: String
    version: Float
}");

        /* When */
        var definition = sut.ParseInputObjectDefinition();

        /* Then */
        Assert.NotNull(definition.Description);
        Assert.Equal("Name", definition.Name);
        Assert.NotNull(definition.Directives);
        Assert.NotNull(definition.Fields);
    }

    [Fact]
    public void SchemaDefinition()
    {
        /* Given */
        var sut = Parser.Create("schema { query: TypeName }");

        /* When */
        var definition = sut.ParseSchemaDefinition();

        /* Then */
        Assert.Null(definition.Description);
        Assert.Null(definition.Directives);
        Assert.Single(definition.Operations, op => op.OperationType == OperationType.Query);
    }

    [Fact]
    public void SchemaDefinition_Description()
    {
        /* Given */
        var sut = Parser.Create(@"
""Description""
schema { query: TypeName }");

        /* When */
        var definition = sut.ParseSchemaDefinition();

        /* Then */
        Assert.Equal("Description", definition.Description);
    }

    [Fact]
    public void SchemaDefinition_Directives()
    {
        /* Given */
        var sut = Parser.Create("schema @a @b { query: TypeName }");

        /* When */
        var definition = sut.ParseSchemaDefinition();

        /* Then */
        Assert.Equal(2, definition.Directives?.Count);
    }

    [Fact]
    public void SchemaDefinition_AllRootTypes()
    {
        /* Given */
        var sut = Parser.Create("schema { query: Query mutation: Mutation subscription: Subscription }");

        /* When */
        var definition = sut.ParseSchemaDefinition();

        /* Then */
        Assert.Null(definition.Description);
        Assert.Null(definition.Directives);
        Assert.Single(definition.Operations, op => op.OperationType == OperationType.Query);
        Assert.Single(definition.Operations, op => op.OperationType == OperationType.Mutation);
        Assert.Single(definition.Operations, op => op.OperationType == OperationType.Subscription);
    }

    [Fact]
    public void TypeSystemDocument_TypeDefinitions()
    {
        /* Given */
        var sut = Parser.Create(@"
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
    ""Field""
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
    ""A""
    A
    ""B""
    B 
}

""""""
Input
""""""
input Input {
    ""Field""
    field: Scalar
}
            ");

        /* When */
        var document = sut.ParseTypeSystemDocument();

        /* Then */
        Assert.NotNull(document.TypeDefinitions);
        Assert.NotEmpty(document.TypeDefinitions);

        // scalar
        Assert.Single(document.TypeDefinitions,
            type => type is ScalarDefinition scalar
                    && scalar.Description == "Scalar");

        // object
        Assert.Single(document.TypeDefinitions,
            type => type is ObjectDefinition obj
                    && obj.Description == "Object");

        // interface
        Assert.Single(document.TypeDefinitions,
            type => type is InterfaceDefinition inf
                    && inf.Description == "Interface"
                    && inf.Fields?.Single().Description == "Field");

        // union
        Assert.Single(document.TypeDefinitions,
            type => type is UnionDefinition union
                    && union.Description == "Union");

        // enum
        Assert.Single(document.TypeDefinitions,
            type => type is EnumDefinition en
                    && en.Description == "Enum"
                    && en.Values?.Last().Description == "B");

        // input
        Assert.Single(document.TypeDefinitions,
            type => type is InputObjectDefinition input
                    && input.Description == "Input"
                    && input.Fields?.Single().Description == "Field");
    }

    [Fact]
    public void TypeSystemDocument_TypeExtensions()
    {
        /* Given */
        var sut = Parser.Create(@"
""""""
Scalar
""""""
extend scalar Scalar

""""""
Object
""""""
extend type Object {
    field: Scalar!
}

""""""
Interface
""""""
extend interface Interface {
    ""Field""
    field: Scalar!
}

""""""
Union
""""""
extend union Union = A | B

""""""
Enum
""""""
extend enum Enum { 
    ""A""
    A
    ""B""
    B 
}

""""""
Input
""""""
extend input Input {
    ""Field""
    field: Scalar
}
            ");

        /* When */
        var document = sut.ParseTypeSystemDocument();

        /* Then */
        Assert.NotNull(document.TypeExtensions);
        Assert.NotEmpty(document.TypeExtensions);

        // scalar
        Assert.Single(document.TypeExtensions,
            type => type.Definition is ScalarDefinition scalar
                    && scalar.Description == "Scalar");

        // object
        Assert.Single(document.TypeExtensions,
            type => type.Definition is ObjectDefinition obj
                    && obj.Description == "Object");

        // interface
        Assert.Single(document.TypeExtensions,
            type => type.Definition is InterfaceDefinition inf
                    && inf.Description == "Interface"
                    && inf.Fields?.Single().Description == "Field");

        // union
        Assert.Single(document.TypeExtensions,
            type => type.Definition is UnionDefinition union
                    && union.Description == "Union");

        // enum
        Assert.Single(document.TypeExtensions,
            type => type.Definition is EnumDefinition en
                    && en.Description == "Enum"
                    && en.Values?.Last().Description == "B");

        // input
        Assert.Single(document.TypeExtensions,
            type => type.Definition is InputObjectDefinition input
                    && input.Description == "Input"
                    && input.Fields?.Single().Description == "Field");
    }
}