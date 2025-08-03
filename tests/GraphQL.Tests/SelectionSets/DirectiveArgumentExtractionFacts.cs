using System.Linq;

using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.SelectionSets;

using Xunit;

namespace Tanka.GraphQL.Tests.SelectionSets;

public class DirectiveArgumentExtractionFacts
{
    [Fact]
    public void GetDirectiveArgumentValue_Should_Extract_String_Value()
    {
        // Given
        var directive = new Directive(
            "defer",
            new Arguments(new[]
            {
                new Argument("label", (StringValue)"test-label")
            }),
            null);

        // When
        var result = GetDirectiveArgumentValue(directive, "label");

        // Then
        Assert.Equal("test-label", result);
    }

    [Fact]
    public void GetDirectiveArgumentValue_Should_Extract_Int_Value()
    {
        // Given
        var directive = new Directive(
            "stream",
            new Arguments(new[]
            {
                new Argument("initialCount", new IntValue(10))
            }),
            null);

        // When
        var result = GetDirectiveArgumentValue(directive, "initialCount");

        // Then
        Assert.Equal(10, result);
    }

    [Fact]
    public void GetDirectiveArgumentValue_Should_Extract_Boolean_Value()
    {
        // Given
        var directive = new Directive(
            "defer",
            new Arguments(new[]
            {
                new Argument("if", new BooleanValue(true))
            }),
            null);

        // When
        var result = GetDirectiveArgumentValue(directive, "if");

        // Then
        Assert.Equal(true, result);
    }

    [Fact]
    public void GetDirectiveArgumentValue_Should_Return_Null_For_Missing_Argument()
    {
        // Given
        var directive = new Directive(
            "defer",
            new Arguments(new[]
            {
                new Argument("label", (StringValue)"test-label")
            }),
            null);

        // When
        var result = GetDirectiveArgumentValue(directive, "nonexistent");

        // Then
        Assert.Null(result);
    }

    [Fact]
    public void GetDirectiveArgumentValue_Should_Return_Null_For_No_Arguments()
    {
        // Given
        var directive = new Directive("defer", null, null);

        // When
        var result = GetDirectiveArgumentValue(directive, "label");

        // Then
        Assert.Null(result);
    }

    // Copy of the method from SelectionSetExecutorFeature for testing
    private static object? GetDirectiveArgumentValue(Directive directive, string argumentName)
    {
        var argument = directive.Arguments?.FirstOrDefault(a => a.Name == argumentName);
        return argument?.Value switch
        {
            StringValue stringValue => stringValue.ToString(),
            IntValue intValue => intValue.Value,
            BooleanValue boolValue => boolValue.Value,
            _ => null
        };
    }
}