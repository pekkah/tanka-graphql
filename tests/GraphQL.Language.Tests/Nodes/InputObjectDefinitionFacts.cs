using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes;

public class InputInputObjectDefinitionFacts
{
    [Fact]
    public void FromBytes()
    {
        /* Given */
        /* When */
        InputObjectDefinition original =
            Encoding.UTF8.GetBytes(@"input Obj {
                    field1: String
                }").AsReadOnlySpan();

        /* Then */
        Assert.Equal("Obj", original.Name);
        Assert.NotNull(original.Fields);
    }

    [Fact]
    public void FromString()
    {
        /* Given */
        /* When */
        InputObjectDefinition original =
            @"input Obj {
                    field1: String
                }";

        /* Then */
        Assert.Equal("Obj", original.Name);
        Assert.NotNull(original.Fields);
    }

    [Fact]
    public void WithDescription()
    {
        /* Given */
        InputObjectDefinition original = @"input Obj";

        /* When */
        var modified = original
            .WithDescription("Description");

        /* Then */
        Assert.Null(original.Description);
        Assert.Equal("Description", modified.Description);
    }

    [Fact]
    public void WithName()
    {
        /* Given */
        InputObjectDefinition original = @"input Obj";

        /* When */
        var modified = original
            .WithName("Renamed");

        /* Then */
        Assert.Equal("Obj", original.Name);
        Assert.Equal("Renamed", modified.Name);
    }

    [Fact]
    public void WithFields()
    {
        /* Given */
        InputObjectDefinition original = @"input Obj";

        /* When */
        var modified = original
            .WithFields(new List<InputValueDefinition>
            {
                "field: Float!"
            });

        /* Then */
        Assert.Null(original.Fields);
        Assert.NotNull(modified.Fields);
        Assert.NotEmpty(modified.Fields);
    }

    [Fact]
    public void WithFields_Modify()
    {
        /* Given */
        InputObjectDefinition original = @"input Obj { field: String }";

        /* When */
        var modified = original
            .WithFields(original
                .Fields?
                .Select(originalField => originalField
                    .WithDescription("Description"))
                .ToList()
            );

        /* Then */
        Assert.NotNull(modified.Fields);
        var field = Assert.Single(modified.Fields);
        Assert.Equal("Description", field?.Description);
    }

    [Fact]
    public void WithDirectives()
    {
        /* Given */
        InputObjectDefinition original = @"input Obj";

        /* When */
        var modified = original
            .WithDirectives(new List<Directive>
            {
                "@a"
            });

        /* Then */
        Assert.Null(original.Directives);
        Assert.NotNull(modified.Directives);
        var a = Assert.Single(modified.Directives);
        Assert.Equal("a", a?.Name);
    }
}