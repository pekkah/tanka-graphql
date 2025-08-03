using System.Collections.Generic;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests.SelectionSets;

public class SimpleDirectiveHandlerFacts
{
    [Fact]
    public void SkipDirectiveHandler_handles_skip_directive()
    {
        // Given
        var handler = new SkipDirectiveHandler();

        // When - test with wrong directive name
        var wrongDirective = new Directive("include", null, null);
        var context = CreateContext(wrongDirective);
        var result = handler.Handle(context);

        // Then
        Assert.False(result.Handled);
    }

    [Fact]
    public void IncludeDirectiveHandler_handles_include_directive()
    {
        // Given
        var handler = new IncludeDirectiveHandler();

        // When - test with wrong directive name
        var wrongDirective = new Directive("skip", null, null);
        var context = CreateContext(wrongDirective);
        var result = handler.Handle(context);

        // Then
        Assert.False(result.Handled);
    }

    [Fact]
    public void DeferDirectiveHandler_handles_defer_directive()
    {
        // Given
        var handler = new DeferDirectiveHandler();

        // When - test with correct directive name
        var directive = new Directive("defer", null, null);
        var context = CreateContext(directive);
        var result = handler.Handle(context);

        // Then
        Assert.True(result.Handled);
        Assert.True(result.Include);
        Assert.NotNull(result.Metadata);
        Assert.True(result.Metadata.ContainsKey("defer"));
    }

    [Fact]
    public void StreamDirectiveHandler_handles_stream_directive()
    {
        // Given
        var handler = new StreamDirectiveHandler();

        // When - test with correct directive name
        var directive = new Directive("stream", null, null);
        var context = CreateContext(directive);
        var result = handler.Handle(context);

        // Then
        Assert.True(result.Handled);
        Assert.True(result.Include);
        Assert.NotNull(result.Metadata);
        Assert.True(result.Metadata.ContainsKey("stream"));
    }

    private DirectiveContext CreateContext(Directive directive)
    {
        // Create a minimal schema
        var schema = new ExecutableSchemaBuilder()
            .Add("Query", new()
            {
                { "field: String", b => b.ResolveAs("value") }
            })
            .Build()
            .GetAwaiter()
            .GetResult();

        // Create a simple field selection
        var fieldSelection = new FieldSelection("field", "field", null, null, null, null);

        return new DirectiveContext
        {
            Schema = schema,
            ObjectDefinition = schema.Query!,
            Selection = fieldSelection,
            Directive = directive,
            CoercedVariableValues = new Dictionary<string, object?>()
        };
    }
}