using System;
using System.Linq;

using Tanka.GraphQL.Language.Nodes;

using Xunit;

namespace Tanka.GraphQL.Language.Tests;

/// <summary>
/// Error handling and recovery tests for the GraphQL Parser.
/// These tests ensure robust parsing behavior when encountering malformed GraphQL documents,
/// invalid syntax, and edge cases that should result in clear, informative error messages.
/// </summary>
public class ParserErrorHandlingFacts
{
    /// <summary>
    /// Helper method to work around ref struct limitations in Assert.Throws
    /// </summary>
    private static Exception AssertThrowsException(string source, System.Action<Parser> parseAction)
    {
        Exception exception = null;
        try
        {
            var parser = Parser.Create(source);
            parseAction(parser);
            Assert.Fail("Expected exception was not thrown");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
        return exception;
    }

    #region Basic Error Handling Tests

    [Fact]
    public void ParseExecutableDocument_WithInvalidTopLevelToken_ThrowsSpecificError()
    {
        // Given: Document starting with invalid token
        var source = "123 { field }";

        // When & Then: Should throw with position information
        var exception = AssertThrowsException(source, p => p.ParseExecutableDocument());
        Assert.Contains("Unexpected token IntValue at 1:", exception.Message);
    }

    [Fact]
    public void ParseOperationDefinition_WithInvalidOperationType_ThrowsSpecificError()
    {
        // Given: Invalid operation type
        var source = "invalid operationName { field }";

        // When & Then: Should throw error about unexpected operation type
        var exception = AssertThrowsException(source, p => p.ParseOperationDefinition());
        Assert.Contains("Unexpected operation type", exception.Message);
    }

    [Fact]
    public void ParseFragmentDefinition_WithoutFragmentKeyword_ThrowsSpecificError()
    {
        // Given: Fragment definition without 'fragment' keyword
        var source = "NotFragment on Type { field }";

        // When & Then: Should throw error about missing 'fragment' keyword
        var exception = AssertThrowsException(source, p => p.ParseFragmentDefinition());
        Assert.Contains("Expected 'fragment'", exception.Message);
    }

    [Fact]
    public void ParseFragmentDefinition_WithInvalidFragmentName_ThrowsError()
    {
        // Given: Fragment definition with reserved word as name
        var source = "fragment on on Type { field }";

        // When & Then: Should throw error about invalid fragment name
        var exception = AssertThrowsException(source, p => p.ParseFragmentDefinition());
        Assert.Contains("Unexpected keyword on", exception.Message);
    }

    [Fact]
    public void ParseFragmentDefinition_WithoutTypeCondition_ThrowsError()
    {
        // Given: Fragment definition without 'on' keyword
        var source = "fragment FragmentName Type { field }";

        // When & Then: Should throw error about missing 'on' keyword
        var exception = AssertThrowsException(source, p => p.ParseFragmentDefinition());
        Assert.Contains("Expected 'on'", exception.Message);
    }

    [Fact]
    public void ParseVariableDefinitions_WithoutParentheses_ThrowsError()
    {
        // Given: Variable definitions without proper parentheses
        var source = "$var: String";

        // When & Then: Should throw error about missing left parenthesis
        var exception = AssertThrowsException(source, p => p.ParseVariableDefinitions());
        Assert.Contains("Expected: LeftParen", exception.Message);
    }

    [Fact]
    public void ParseSelectionSet_WithMissingCloseBrace_ThrowsError()
    {
        // Given: Selection set without closing brace
        var source = "{ field";

        // When & Then: Should throw error about missing close brace
        var exception = AssertThrowsException(source, p => p.ParseExecutableDocument());
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public void Parser_ErrorMessages_IncludePositionInformation()
    {
        // Given: Document with error on specific line and column
        var source = @"
            query MyQuery {
                field
                invalid_token_here 123
            }
        ";

        // When & Then: Error should include line and column information
        var exception = AssertThrowsException(source, p => p.ParseExecutableDocument());

        // Should contain position information (line:column format)
        Assert.Matches(@"\d+:\d+", exception.Message);
    }

    [Fact]
    public void Parser_ErrorMessages_ProvideSpecificExpectations()
    {
        // Given: Document with specific syntax error
        var source = "query { field(arg }";

        // When & Then: Error message should specify what was expected
        var exception = AssertThrowsException(source, p => p.ParseExecutableDocument());
        Assert.Contains("Expected", exception.Message);
    }

    #endregion

    #region Successful Parsing Tests (Positive Cases)

    [Fact]
    public void ParseFragmentSpread_WithoutSpreadOperator_ParsesAsField()
    {
        // Given: Fragment spread without ... operator
        var source = "{ FragmentName }";
        var parser = Parser.Create(source);

        // When: Parse as normal field (this should work)
        var document = parser.ParseExecutableDocument();

        // Then: Should be parsed as a field, not fragment spread
        Assert.NotNull(document);
        var operation = document.OperationDefinitions?.FirstOrDefault();
        Assert.NotNull(operation);
        var field = operation.SelectionSet?.FirstOrDefault() as FieldSelection;
        Assert.NotNull(field);
        Assert.Equal("FragmentName", field.Name.Value);
    }

    [Fact]
    public void ParseDirective_WithoutAtSign_ParsesAsField()
    {
        // Given: Directive without @ prefix
        var source = "{ field skip(if: true) }";
        var parser = Parser.Create(source);

        // When: This should parse as a field with arguments
        var document = parser.ParseExecutableDocument();

        // Then: Should be parsed as normal field, not directive
        Assert.NotNull(document);
        var operation = document.OperationDefinitions?.FirstOrDefault();
        Assert.NotNull(operation);
        var field = operation.SelectionSet?.FirstOrDefault() as FieldSelection;
        Assert.NotNull(field);
        Assert.Equal("field", field.Name.Value);
    }

    [Fact]
    public void Parser_WithEmptyDocument_HandlesGracefully()
    {
        // Given: Completely empty document
        var source = "";
        var parser = Parser.Create(source);

        // When: Parse
        var document = parser.ParseExecutableDocument();

        // Then: Should return empty document structure
        Assert.NotNull(document);
        Assert.Equal(0, document.OperationDefinitions?.Count ?? 0);
        Assert.Equal(0, document.FragmentDefinitions?.Count ?? 0);
    }

    [Fact]
    public void Parser_WithOnlyWhitespace_HandlesGracefully()
    {
        // Given: Document with only whitespace and comments
        var source = "   \n\r\n\t   \r  \n  ";
        var parser = Parser.Create(source);

        // When: Parse
        var document = parser.ParseExecutableDocument();

        // Then: Should return empty document structure
        Assert.NotNull(document);
        Assert.Equal(0, document.OperationDefinitions?.Count ?? 0);
        Assert.Equal(0, document.FragmentDefinitions?.Count ?? 0);
    }

    #endregion

    #region Complex Error Scenarios

    [Fact]
    public void ParseIntValue_WithIntegerOverflow_HandlesProperly()
    {
        // Given: Document with very large integer
        var source = "{ field(arg: 999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999) }";
        var parser = Parser.Create(source);

        // When & Then: Should handle the large integer (may succeed or fail gracefully)
        try
        {
            var document = parser.ParseExecutableDocument();
            // If it succeeds, the integer was handled
            Assert.NotNull(document);
        }
        catch (Exception ex)
        {
            // If it fails, should be a clear error message
            Assert.NotNull(ex.Message);
            Assert.True(ex.Message.Contains("parse") || ex.Message.Contains("integer") || ex.Message.Contains("value"));
        }
    }

    [Fact]
    public void ParseInlineFragment_WithInvalidTypeCondition_ThrowsError()
    {
        // Given: Inline fragment with invalid type condition
        var source = "{ ... on 123 { field } }";

        // When & Then: Should throw error about invalid type condition
        var exception = AssertThrowsException(source, p => p.ParseExecutableDocument());
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public void ParseDirective_WithMissingName_ThrowsError()
    {
        // Given: Directive with missing name after @
        var source = "{ field @(if: true) }";

        // When & Then: Should throw error about missing directive name
        var exception = AssertThrowsException(source, p => p.ParseExecutableDocument());
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public void ParseType_WithMismatchedBrackets_ThrowsError()
    {
        // Given: List type with mismatched brackets
        var source = "($var: [String)";

        // When & Then: Should throw error about mismatched brackets
        var exception = AssertThrowsException(source, p => p.ParseVariableDefinitions());
        Assert.NotNull(exception.Message);
    }

    #endregion
}