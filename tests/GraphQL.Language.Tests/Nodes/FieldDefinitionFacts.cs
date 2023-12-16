using System.Text;

using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes;

public class FieldDefinitionFacts
{
    [Fact]
    public void FromBytes()
    {
        /* Given */
        /* When */
        FieldDefinition original = "field: String"u8;

        /* Then */
        Assert.Equal("field", original.Name);
        Assert.IsType<NamedType>(original.Type);
    }

    [Fact]
    public void FromString()
    {
        /* Given */
        /* When */
        FieldDefinition original = "field: String";

        /* Then */
        Assert.Equal("field", original.Name);
        Assert.IsType<NamedType>(original.Type);
    }

    [Fact]
    public void WithDescription()
    {
        /* Given */
        FieldDefinition original = @"field: Int";

        /* When */
        FieldDefinition withDescription = original
            .WithDescription("Description");

        /* Then */
        Assert.Null(original.Description);
        Assert.Equal("Description", withDescription.Description);
    }

    [Fact]
    public void WithName()
    {
        /* Given */
        FieldDefinition original = @"field: Int";

        /* When */
        FieldDefinition renamed = original
            .WithName("renamed");

        /* Then */
        Assert.Equal("field", original.Name);
        Assert.Equal("renamed", renamed.Name);
    }


    [Fact]
    public void WithArguments()
    {
        /* Given */
        FieldDefinition original = @"field: Int";

        /* When */
        FieldDefinition modified = original
            .WithArguments(new InputValueDefinition[] { "id: ID!" });

        /* Then */
        Assert.NotNull(modified.Arguments);
        InputValueDefinition id = Assert.Single(modified.Arguments);
        Assert.Equal("id", id?.Name);
    }

    [Fact]
    public void WithType()
    {
        /* Given */
        FieldDefinition original = @"field: Int";

        /* When */
        FieldDefinition modified = original
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
        FieldDefinition original = @"field: Int";

        /* When */
        FieldDefinition modified = original
            .WithDirectives(new[] { new Directive("a", null) });

        /* Then */
        Assert.NotNull(modified.Directives);
        Directive a = Assert.Single(modified.Directives);
        Assert.Equal("a", a?.Name);
    }
}