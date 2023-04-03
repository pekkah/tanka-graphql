using System.Collections.Generic;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes;

public class InputValueDefinitionFacts
{
    [Fact]
    public void FromBytes()
    {
        /* Given */
        /* When */
        InputValueDefinition original = Encoding.UTF8.GetBytes("field: ENUM")
            .AsReadOnlySpan();

        /* Then */
        Assert.Equal("field", original.Name);
        Assert.IsType<NamedType>(original.Type);
    }

    [Fact]
    public void FromString()
    {
        /* Given */
        /* When */
        InputValueDefinition original = "field: ENUM";

        /* Then */
        Assert.Equal("field", original.Name);
        Assert.IsType<NamedType>(original.Type);
    }

    [Fact]
    public void WithDescription()
    {
        /* Given */
        InputValueDefinition original = "field: ENUM!";

        /* When */
        var modified = original
            .WithDescription("Description");

        /* Then */
        Assert.Equal("Description", modified.Description);
    }

    [Fact]
    public void WithName()
    {
        /* Given */
        InputValueDefinition original = "field: ENUM!";

        /* When */
        var modified = original
            .WithName("b");

        /* Then */
        Assert.Equal("field", original.Name);
        Assert.Equal("b", modified.Name);
    }

    [Fact]
    public void WithType()
    {
        /* Given */
        InputValueDefinition original = @"field: Int";

        /* When */
        var modified = original
            .WithType("String!");

        /* Then */
        var nonNull = Assert.IsType<NonNullType>(modified.Type);
        var named = Assert.IsType<NamedType>(nonNull.OfType);
        Assert.Equal("String", named.Name);
    }

    [Fact]
    public void WithDirectives()
    {
        /* Given */
        InputValueDefinition original = @"field: Int";

        /* When */
        var modified = original
            .WithDirectives(new List<Directive>
            {
                "@a"
            });

        /* Then */
        Assert.NotNull(modified.Directives);
        var a = Assert.Single(modified.Directives);
        Assert.Equal("a", a?.Name);
    }
}