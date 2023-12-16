using Tanka.GraphQL.Language.Nodes.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes;

public class DirectiveDefinitionFacts
{
    [Fact]
    public void FromBytes()
    {
        /* Given */
        /* When */
        DirectiveDefinition original = "directive @a(x: Int, y: Float) on FIELD"u8;

        /* Then */
        Assert.Equal("a", original.Name);
    }

    [Fact]
    public void FromString()
    {
        /* Given */
        /* When */
        DirectiveDefinition original = "directive @a(x: Int, y: Float) on FIELD";

        /* Then */
        Assert.Equal("a", original.Name);
    }

    [Fact]
    public void WithDescription()
    {
        /* Given */
        DirectiveDefinition original = @"directive @a on SCHEMA";

        /* When */
        DirectiveDefinition modified = original
            .WithDescription("Description");

        /* Then */
        Assert.Null(original.Description);
        Assert.Equal("Description", modified.Description);
    }

    [Fact]
    public void WithName()
    {
        /* Given */
        DirectiveDefinition original = @"directive @a on SCHEMA";

        /* When */
        DirectiveDefinition modified = original
            .WithName("b");

        /* Then */
        Assert.Equal("a", original.Name);
        Assert.Equal("b", modified.Name);
    }

    [Fact]
    public void WithArguments()
    {
        /* Given */
        DirectiveDefinition original = @"directive @a on SCHEMA";

        /* When */
        DirectiveDefinition modified = original
            .WithArguments(new InputValueDefinition[] { "x: Int" });

        /* Then */
        Assert.Equal(1, modified.Arguments?.Count);
    }

    [Fact]
    public void WithDirectiveLocations()
    {
        /* Given */
        DirectiveDefinition original = @"directive @a on SCHEMA";

        /* When */
        DirectiveDefinition modified = original
            .WithDirectiveLocations(new[] { "FIELD" });

        /* Then */
        string location = Assert.Single(modified.DirectiveLocations);
        Assert.Equal("FIELD", location);
    }
}