using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Tests.SelectionSets;

public class DeferStreamDirectiveHandlerFacts
{
    [Fact]
    public async Task DeferDirectiveHandler_Should_Store_Directive_With_Label()
    {
        // Given
        var handler = new DeferDirectiveHandler();
        var directive = new Directive(
            "defer",
            new Arguments(new[]
            {
                new Argument("label", (StringValue)"test-label")
            }),
            null);

        var context = new DirectiveContext
        {
            Schema = await new SchemaBuilder().Add("type Query { hello: String }").Build(),
            ObjectDefinition = new ObjectDefinition("Test", null, null, null, null),
            Selection = new InlineFragment(null, null, new SelectionSet(new ISelection[0]), null),
            Directive = directive,
            CoercedVariableValues = null
        };

        // When
        var result = handler.Handle(context);

        // Then
        Assert.True(result.Handled);
        Assert.True(result.Include);
        Assert.NotNull(result.Metadata);
        Assert.True(result.Metadata.ContainsKey("defer"));

        var storedDirective = (Directive)result.Metadata["defer"];
        Assert.Equal("defer", storedDirective.Name.Value);

        // Verify label argument is preserved
        var labelArg = storedDirective.Arguments?.FirstOrDefault(a => a.Name.Value == "label");
        Assert.NotNull(labelArg);
        Assert.IsType<StringValue>(labelArg.Value);
        Assert.Equal("test-label", ((StringValue)labelArg.Value).ToString());
    }

    [Fact]
    public async Task DeferDirectiveHandler_Should_Handle_If_False()
    {
        // Given
        var handler = new DeferDirectiveHandler();
        var directive = new Directive(
            "defer",
            new Arguments(new[]
            {
                new Argument("if", new BooleanValue(false))
            }),
            null);

        var context = new DirectiveContext
        {
            Schema = await new SchemaBuilder().Add("type Query { hello: String }").Build(),
            ObjectDefinition = new ObjectDefinition("Test", null, null, null, null),
            Selection = new InlineFragment(null, null, new SelectionSet(new ISelection[0]), null),
            Directive = directive,
            CoercedVariableValues = null
        };

        // When
        var result = handler.Handle(context);

        // Then
        Assert.True(result.Handled);
        Assert.True(result.Include);
        Assert.Null(result.Metadata); // No metadata when if=false
    }

    [Fact]
    public async Task StreamDirectiveHandler_Should_Store_Directive_With_Label_And_InitialCount()
    {
        // Given
        var handler = new StreamDirectiveHandler();
        var directive = new Directive(
            "stream",
            new Arguments(new[]
            {
                new Argument("label", (StringValue)"stream-label"),
                new Argument("initialCount", new IntValue(5))
            }),
            null);

        var context = new DirectiveContext
        {
            Schema = await new SchemaBuilder().Add("type Query { hello: String }").Build(),
            ObjectDefinition = new ObjectDefinition("Test", null, null, null, null),
            Selection = new FieldSelection("", "testField", null, null, null, null),
            Directive = directive,
            CoercedVariableValues = null
        };

        // When
        var result = handler.Handle(context);

        // Then
        Assert.True(result.Handled);
        Assert.True(result.Include);
        Assert.NotNull(result.Metadata);
        Assert.True(result.Metadata.ContainsKey("stream"));

        var storedDirective = (Directive)result.Metadata["stream"];
        Assert.Equal("stream", storedDirective.Name.Value);

        // Verify arguments are preserved
        var labelArg = storedDirective.Arguments?.FirstOrDefault(a => a.Name.Value == "label");
        Assert.NotNull(labelArg);
        Assert.Equal("stream-label", ((StringValue)labelArg.Value).ToString());

        var initialCountArg = storedDirective.Arguments?.FirstOrDefault(a => a.Name.Value == "initialCount");
        Assert.NotNull(initialCountArg);
        Assert.Equal(5, ((IntValue)initialCountArg.Value).Value);
    }
}