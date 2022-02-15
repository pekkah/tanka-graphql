using System.Linq;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests;

public class TankaImportSyntaxFacts
{
    [Fact]
    public void Import()
    {
        /* Given */
        var sut = Parser.Create(@"
""""""
tanka_import from ""./from""
""""""
");

        /* When */
        var import = sut.ParseTankaImport();

        /* Then */
        Assert.Null(import.Types);
        Assert.Equal("./from", import.From);
    }

    [Fact]
    public void Import_Types()
    {
        /* Given */
        var sut = Parser.Create(@"
""""""
tanka_import A,B,C from ""./from""
""""""
");

        /* When */
        var import = sut.ParseTankaImport();

        /* Then */
        Assert.NotNull(import.Types);
        Assert.Single(import.Types, t => t == "A");
        Assert.Single(import.Types, t => t == "B");
        Assert.Single(import.Types, t => t == "C");
        Assert.Equal("./from", import.From);
    }

    [Fact]
    public void Imports()
    {
        /* Given */
        var sut = Parser.Create(@"
""""""
tanka_import from ""./from""
tanka_import Type1 from ""./from2""
""""""
");

        /* When */
        var document = sut.ParseTypeSystemDocument();

        /* Then */
        Assert.NotNull(document.Imports);
        Assert.Equal(2, document.Imports.Count);
    }

    [Fact]
    public void Imports_are_not_descriptions()
    {
        /* Given */
        var sut = Parser.Create(@"
""""""
tanka_import from ""./from""
tanka_import Type1 from ""./from2""
""""""
type Person {
    field1: Int
}
");

        /* When */
        var document = sut.ParseTypeSystemDocument();

        /* Then */
        Assert.NotNull(document.Imports);
        Assert.Equal(2, document.Imports.Count);
        var person = Assert.Single(document?.TypeDefinitions?.OfType<ObjectDefinition>() ??
                                   Enumerable.Empty<ObjectDefinition>());
        Assert.Null(person?.Description);
    }

    [Fact]
    public void Imports_are_not_descriptions2()
    {
        /* Given */
        var sut = Parser.Create(@"
""""""
tanka_import from ""./from""
tanka_import Type1 from ""./from2""
""""""
""""""
Description
""""""
type Person {
    field1: Int
}
");

        /* When */
        var document = sut.ParseTypeSystemDocument();

        /* Then */
        Assert.NotNull(document.Imports);
        Assert.Equal(2, document.Imports.Count);
        var person = Assert.Single(document?.TypeDefinitions?.OfType<ObjectDefinition>() ??
                                   Enumerable.Empty<ObjectDefinition>());
        Assert.Equal("Description", person?.Description);
    }
}