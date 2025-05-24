using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace Tanka.GraphQL.Language.Tests;

public class LexerFacts
{
    [Theory]
    [InlineData(Constants.ExclamationMark, TokenKind.ExclamationMark)]
    [InlineData(Constants.Dollar, TokenKind.Dollar)]
    [InlineData(Constants.Ampersand, TokenKind.Ampersand)]
    [InlineData(Constants.LeftParenthesis, TokenKind.LeftParenthesis)]
    [InlineData(Constants.RightParenthesis, TokenKind.RightParenthesis)]
    [InlineData(Constants.Colon, TokenKind.Colon)]
    [InlineData(Constants.Equal, TokenKind.Equal)]
    [InlineData(Constants.At, TokenKind.At)]
    [InlineData(Constants.LeftBracket, TokenKind.LeftBracket)]
    [InlineData(Constants.RightBracket, TokenKind.RightBracket)]
    [InlineData(Constants.LeftBrace, TokenKind.LeftBrace)]
    [InlineData(Constants.Pipe, TokenKind.Pipe)]
    [InlineData(Constants.RightBrace, TokenKind.RightBrace)]
    public void ReadPunctuator(byte punctuator, TokenKind expectedKind)
    {
        /* Given */
        var source = new ReadOnlySpan<byte>(new[]
        {
            punctuator
        });

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(expectedKind, sut.Kind);
    }

    [Fact]
    public void IgnoreWhitespace()
    {
        /* Given */
        var source = "   {";

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.LeftBrace, sut.Kind);
        Assert.Equal(1, sut.Line);
        Assert.Equal(4, sut.Column);
    }

    [Fact]
    public void ReadBom()
    {
        /* Given */
        var source = Encoding.UTF8.GetPreamble();

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(Constants.Bom.Length - 1, sut.Position);
        Assert.Equal(TokenKind.End, sut.Kind);
    }

    [Conditional("GQL_COMMENTS")]
    [Theory]
    [InlineData("# comment")]
    [InlineData("#comment")]
    [InlineData("# comment\n")]
    [InlineData("#comment\r")]
    [InlineData("#comment\r\n")]
    public void ReadComment(string comment)
    {
        /* Given */
        var source = comment;

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

#if GQL_COMMENTS
            /* Then */
            Assert.Equal(TokenKind.Comment, sut.Kind);
            Assert.Equal("comment", Encoding.UTF8.GetString(sut.Value));
            Assert.Equal(1, sut.Column);
#endif
    }

#if GQL_COMMENTS
    [Fact]
    public void ReadComment_AtStartOfInput_ThenToken()
    {
        /* Given */
        var source = "#comment1\nquery {}";
        var sut = Lexer.Create(source);

        /* When & Then */
        // Comment
        Assert.True(sut.Advance());
        Assert.Equal(TokenKind.Comment, sut.Kind);
        Assert.Equal("comment1", Encoding.UTF8.GetString(sut.Value));
        Assert.Equal(1, sut.Line);
        Assert.Equal(1, sut.Column); // '#' is at col 1

        // Query keyword
        Assert.True(sut.Advance());
        Assert.Equal(TokenKind.Name, sut.Kind);
        Assert.Equal("query", Encoding.UTF8.GetString(sut.Value));
        Assert.Equal(2, sut.Line);
        Assert.Equal(1, sut.Column);
    }

    [Fact]
    public void ReadComment_AtEndOfInput_AfterToken()
    {
        /* Given */
        var source = "query {} #comment1";
        var sut = Lexer.Create(source);

        /* When & Then */
        Assert.True(sut.Advance()); // query
        Assert.Equal(TokenKind.Name, sut.Kind);
        Assert.Equal(1, sut.Line);
        Assert.Equal(1, sut.Column);

        Assert.True(sut.Advance()); // {
        Assert.Equal(TokenKind.LeftBrace, sut.Kind);
        Assert.Equal(1, sut.Line);
        Assert.Equal(7, sut.Column); // "query " is 6 chars, { is at 7

        Assert.True(sut.Advance()); // }
        Assert.Equal(TokenKind.RightBrace, sut.Kind);
        Assert.Equal(1, sut.Line);
        Assert.Equal(8, sut.Column);

        // Comment
        Assert.True(sut.Advance());
        Assert.Equal(TokenKind.Comment, sut.Kind);
        Assert.Equal("comment1", Encoding.UTF8.GetString(sut.Value));
        Assert.Equal(1, sut.Line);
        Assert.Equal(10, sut.Column); // "query {} " is 9 chars, # is at 10

        // EOF
        Assert.False(sut.Advance());
        Assert.Equal(TokenKind.End, sut.Kind);
    }

    [Theory]
    [InlineData("query #comment1\nMyQuery {}", 1, 7, "comment1", 2, 1, TokenKind.Name, "MyQuery")] // query #c1 \n MyQuery
    [InlineData("field #comment1\n(arg: 1)", 1, 7, "comment1", 2, 1, TokenKind.LeftParenthesis, "(")] // field #c1 \n (
    [InlineData("{ field1 #comment1\n field2 }", 1, 10, "comment1", 2, 2, TokenKind.Name, "field2")] // { field1 #c1 \n  field2 (note leading space for field2)
    public void ReadComment_BetweenTokensOnDifferentLines(
        string sourceInput,
        int commentLine, int commentColumn, string commentValue,
        int nextTokenLine, int nextTokenColumn, TokenKind nextTokenKind, string nextTokenValue)
    {
        /* Given */
        var sut = Lexer.Create(sourceInput);

        /* When & Then */
        // Advance past first token(s) to get to the comment
        // This logic assumes first token is simple and on line 1.
        Assert.True(sut.Advance()); // First part of the input e.g. "query", "field", "{"
        if (sut.Kind == TokenKind.LeftBrace) Assert.True(sut.Advance()); // Consume "field1" if first token was "{"

        // Comment
        Assert.True(sut.Advance());
        Assert.Equal(TokenKind.Comment, sut.Kind);
        Assert.Equal(commentValue, Encoding.UTF8.GetString(sut.Value));
        Assert.Equal(commentLine, sut.Line);
        Assert.Equal(commentColumn, sut.Column);

        // Next Token
        Assert.True(sut.Advance());
        Assert.Equal(nextTokenKind, sut.Kind);
        Assert.Equal(nextTokenValue, Encoding.UTF8.GetString(sut.Value));
        Assert.Equal(nextTokenLine, sut.Line);
        Assert.Equal(nextTokenColumn, sut.Column);
    }
    
    [Fact]
    public void ReadComment_BetweenArgumentsInList()
    {
        /* Given */
        var source = "(arg1: 1 #comment1\n arg2: 2)";
        var sut = Lexer.Create(source);

        /* When & Then */
        Assert.True(sut.Advance()); // (
        Assert.True(sut.Advance()); // arg1
        Assert.True(sut.Advance()); // :
        Assert.True(sut.Advance()); // 1 (IntValue)

        // Comment
        Assert.True(sut.Advance());
        Assert.Equal(TokenKind.Comment, sut.Kind);
        Assert.Equal("comment1", Encoding.UTF8.GetString(sut.Value));
        Assert.Equal(1, sut.Line);
        Assert.Equal(10, sut.Column); // Corrected: "(arg1: 1 " is 9 chars, # is at 10. String: "(arg1: 1 " is 9. So # is 10.

        // Next Token (arg2)
        Assert.True(sut.Advance());
        Assert.Equal(TokenKind.Name, sut.Kind);
        Assert.Equal("arg2", Encoding.UTF8.GetString(sut.Value));
        Assert.Equal(2, sut.Line);
        Assert.Equal(2, sut.Column); // Note leading space on " arg2"
    }


    [Theory]
    [InlineData("#comment1\n#comment2  \nquery {}", "comment1", 1, 1, "comment2  ", 2, 1, "query", 3, 1)]
    [InlineData("query\n#comment1\n#comment2\n{}", "comment1", 2, 1, "comment2", 3, 1, "{", 4, 1)]
    public void ReadComment_Consecutive(
        string sourceInput,
        string comment1Value, int comment1Line, int comment1Column,
        string comment2Value, int comment2Line, int comment2Column,
        string nextTokenValue, int nextTokenLine, int nextTokenColumn)
    {
        /* Given */
        var sut = Lexer.Create(sourceInput);

        /* When & Then */
        if (sourceInput.StartsWith("query")) // Skip initial "query" if present
        {
            Assert.True(sut.Advance());
            Assert.Equal(TokenKind.Name, sut.Kind);
            Assert.Equal("query", Encoding.UTF8.GetString(sut.Value));
        }

        // Comment 1
        Assert.True(sut.Advance());
        Assert.Equal(TokenKind.Comment, sut.Kind);
        Assert.Equal(comment1Value, Encoding.UTF8.GetString(sut.Value));
        Assert.Equal(comment1Line, sut.Line);
        Assert.Equal(comment1Column, sut.Column);

        // Comment 2
        Assert.True(sut.Advance());
        Assert.Equal(TokenKind.Comment, sut.Kind);
        Assert.Equal(comment2Value, Encoding.UTF8.GetString(sut.Value));
        Assert.Equal(comment2Line, sut.Line);
        Assert.Equal(comment2Column, sut.Column);
        
        // Next Token
        Assert.True(sut.Advance());
        Assert.Equal(nextTokenValue == "{" ? TokenKind.LeftBrace : TokenKind.Name, sut.Kind); // Basic check
        Assert.Equal(nextTokenValue, Encoding.UTF8.GetString(sut.Value));
        Assert.Equal(nextTokenLine, sut.Line);
        Assert.Equal(nextTokenColumn, sut.Column);
    }

    [Theory]
    [InlineData("#\nquery {}", "", 1, 1, "query", 2, 1)] // Empty comment
    [InlineData("#   \nquery {}", "   ", 1, 1, "query", 2, 1)] // Comment with only spaces
    [InlineData("# leadingSpace\nquery {}", "leadingSpace", 1, 1, "query", 2, 1)] // Content has no leading space due to IgnoreWhitespace
    [InlineData("#commentTrailingSpace   \nquery {}", "commentTrailingSpace   ", 1, 1, "query", 2, 1)]
    public void ReadComment_EmptyAndWhitespaceVariants(
        string sourceInput, string commentValue, int commentLine, int commentColumn,
        string nextTokenValue, int nextTokenLine, int nextTokenColumn)
    {
        /* Given */
        var sut = Lexer.Create(sourceInput);

        /* When & Then */
        // Comment
        Assert.True(sut.Advance());
        Assert.Equal(TokenKind.Comment, sut.Kind);
        Assert.Equal(commentValue, Encoding.UTF8.GetString(sut.Value));
        Assert.Equal(commentLine, sut.Line);
        Assert.Equal(commentColumn, sut.Column);

        // Next Token
        Assert.True(sut.Advance());
        Assert.Equal(TokenKind.Name, sut.Kind); // Assuming next token is always a Name for simplicity
        Assert.Equal(nextTokenValue, Encoding.UTF8.GetString(sut.Value));
        Assert.Equal(nextTokenLine, sut.Line);
        Assert.Equal(nextTokenColumn, sut.Column);
    }
#endif

    [Fact]
    public void ReadSpreadPunctuator()
    {
        /* Given */
        var source = "...";

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.Spread, sut.Kind);
        Assert.Equal(1, sut.Column);
    }

    [Fact]
    public void ReadSpreadPunctuator2()
    {
        /* Given */
        var source = "{...}";

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.Spread, sut.Kind);
        Assert.Equal(2, sut.Column);
    }

    [Fact]
    public void ReadTokenAfterSpreadPunctuator()
    {
        /* Given */
        var source = "{...}";

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();
        sut.Advance();
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.RightBrace, sut.Kind);
        Assert.Equal(5, sut.Column);
    }

    [Fact]
    public void Column()
    {
        /* Given */
        var source = "...{}...";

        var sut = Lexer.Create(source);

        var actual = new List<(TokenKind Kind, long Line, long Column)>();

        /* When */
        while (sut.Advance())
            actual.Add((sut.Kind, sut.Line, sut.Column));

        /* Then */
        Assert.Contains(actual, token =>
            token.Kind == TokenKind.LeftBrace
            && token.Column == 4);
    }

    [Theory]
    [InlineData("007", "Invalid number: leading zero followed by other digits", 0)]
    [InlineData("01", "Invalid number: leading zero followed by other digits", 0)]
    [InlineData("1.2.3", "Invalid number: multiple decimal points", 0)]
    [InlineData("1e2e3", "Invalid number: multiple exponent parts", 0)]
    [InlineData("1.0E2E3", "Invalid number: multiple exponent parts", 0)]
    [InlineData("1e+", "Invalid number: exponent part missing digits", 0)]
    [InlineData("1.0E-", "Invalid number: exponent part missing digits", 0)]
    [InlineData("1.", "Invalid number: decimal part missing digits", 0)]
    public void ReadInvalidNumber_ThrowsException_WithCorrectMessageAndLocation(string numberString, string expectedErrorMessagePart, int expectedStartIndex)
    {
        /* Given */
        var source = numberString;
        var lexerInstance = Lexer.Create(source);
        Exception? actualException = null;

        /* When */
        try
        {
            lexerInstance.Advance();
        }
        catch (Exception ex)
        {
            actualException = ex;
        }

        /* Then */
        Assert.NotNull(actualException);
        // Ensure it's a System.Exception as per problem description for "00"
        // (or a more specific type if that's what is actually thrown for these cases)
        Assert.IsAssignableFrom<Exception>(actualException); 
        Assert.Contains(expectedErrorMessagePart, actualException!.Message); // Use null suppression due to Assert.NotNull above
        Assert.Contains($"starting at {expectedStartIndex}", actualException.Message);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("_name")]
    [InlineData("__name")]
    [InlineData("d123")]
    [InlineData("_123")]
    public void ReadName(string name)
    {
        /* Given */
        var source = name;

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.Name, sut.Kind);
        Assert.Equal(1, sut.Column);
        Assert.Equal(name, Encoding.UTF8.GetString(sut.Value));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("-123")]
    [InlineData("0")]
    public void ReadInteger(string integer)
    {
        /* Given */
        var source = integer;

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.IntValue, sut.Kind);
        Assert.Equal(1, sut.Column);
        Assert.Equal(integer, Encoding.UTF8.GetString(sut.Value));
    }

    [Theory]
    [InlineData("123.123")]
    [InlineData("-123.123")]
    [InlineData("123e123")]
    [InlineData("-123e123")]
    [InlineData("123.123e20")]
    public void ReadFloat(string floatValue)
    {
        /* Given */
        var source = floatValue;

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.FloatValue, sut.Kind);
        Assert.Equal(1, sut.Column);
        Assert.Equal(floatValue, Encoding.UTF8.GetString(sut.Value));
    }

    [Theory]
    [InlineData("\"\"", "")]
    [InlineData("\"test\"", "test")]
    [InlineData("\"test \\\"\"", "test \"")] // Corrected expected value: \" should unescape to "
    public void ReadString(string stringValue, string expectedValue)
    {
        /* Given */
        var source = stringValue;

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.StringValue, sut.Kind);
        Assert.Equal(1, sut.Column);
        Assert.Equal(expectedValue, Encoding.UTF8.GetString(sut.Value));
    }

    [Theory]
    [InlineData("\"\"\"\"\"\"", "")]
    [InlineData("\"\"\"test\"\"\"", "test")]
    [InlineData("\"\"\"test \\\"\"\"\"\"\"", "test \"\"\"")] // Corrected: \""" unescapes to """
    [InlineData("\"\"\"test test test\"\"\"", "test test test")]
    public void ReadBlockStringValue(string stringValue, string expectedValue)
    {
        /* Given */
        var source = stringValue;

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.BlockStringValue, sut.Kind);
        Assert.Equal(1, sut.Column);
        Assert.Equal(expectedValue, Encoding.UTF8.GetString(sut.Value));
    }

    [Theory]
    [InlineData("\"test\"", "test")]
    public void ReadStringValue(string stringValue, string expectedValue)
    {
        /* Given */
        var source = stringValue;

        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.StringValue, sut.Kind);
        Assert.Equal(1, sut.Column);
        Assert.Equal(expectedValue, Encoding.UTF8.GetString(sut.Value));
    }

    [Theory]
    [InlineData("\"\\n\"", "\n")]
    [InlineData("\"\\r\"", "\r")]
    [InlineData("\"\\t\"", "\t")]
    [InlineData("\"\\b\"", "\b")]
    [InlineData("\"\\f\"", "\f")]
    [InlineData("\"\\\\\"", "\\")]
    [InlineData("\"\\/\"", "/")]
    [InlineData("\"\\\"\"", "\"")]
    [InlineData("\"a\\nb\\tc\"", "a\nb\tc")]
    [InlineData("\"prefix\\nsuffix\"", "prefix\nsuffix")]
    public void ReadString_WithStandardEscapeSequences(string inputString, string expectedValue)
    {
        /* Given */
        var source = inputString;
        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.StringValue, sut.Kind);
        Assert.Equal(1, sut.Column); // Assuming simple strings not crossing lines for column count
        // The actual Value of the string token is what's between the quotes, after unescaping.
        // The current Lexer.cs does not unescape. This test will fail until Lexer.cs is fixed.
        Assert.Equal(expectedValue, Encoding.UTF8.GetString(sut.Value));
    }

    [Theory]
    [InlineData("\"\\u0020\"", " ")]
    [InlineData("\"\\u000A\"", "\n")]
    [InlineData("\"\\uFFFF\"", "\uFFFF")]
    [InlineData("\"text \\u0041 text\"", "text A text")]
    [InlineData("\"\\u004a\\u004B\"", "JK")]
    public void ReadString_WithValidUnicodeEscapeSequences(string inputString, string expectedValue)
    {
        /* Given */
        var source = inputString;
        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.StringValue, sut.Kind);
        Assert.Equal(1, sut.Column);
        // The current Lexer.cs does not unescape Unicode sequences. This test will fail.
        Assert.Equal(expectedValue, Encoding.UTF8.GetString(sut.Value));
    }

    [Theory]
    [InlineData("\"\\u123\"", "Invalid Unicode escape sequence", 0)] // Too few digits
    [InlineData("\"\\u123X\"", "Invalid Unicode escape sequence", 0)] // Invalid hex char
    [InlineData("\"\\u\"", "Invalid Unicode escape sequence", 0)]     // Incomplete
    [InlineData("\"text \\u12\"", "Invalid Unicode escape sequence", 0)] // Incomplete in context
    public void ReadString_WithInvalidUnicodeEscapeSequence_ThrowsException(string inputString, string expectedErrorMessagePart, int expectedTokenStartOffset)
    {
        /* Given */
        var source = inputString;
        var lexerInstance = Lexer.Create(source);
        Exception? actualException = null;

        /* When */
        try
        {
            lexerInstance.Advance();
        }
        catch (Exception ex)
        {
            actualException = ex;
        }

        /* Then */
        Assert.NotNull(actualException);
        // Current Lexer.cs likely throws "StringValue at {Start} is not terminated." or parses incorrectly,
        // rather than a specific Unicode error. This assertion will likely fail or catch a different error.
        Assert.IsAssignableFrom<Exception>(actualException);
        Assert.Contains(expectedErrorMessagePart, actualException!.Message);
        // Assert.Contains($"starting at {expectedTokenStartOffset}", actualException.Message); // Location might not be in this format for string errors
    }

    [Theory]
    [InlineData("\"\\q\"", "\\q")] // Spec: "All other uses of \ within a StringValue are descriptive of the \ character itself"
    [InlineData("\"a\\qb\"", "a\\qb")]
    public void ReadString_WithNonSpecStandardEscape_ParsesAsLiteral(string inputString, string expectedValue)
    {
        /* Given */
        var source = inputString;
        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.StringValue, sut.Kind);
        Assert.Equal(1, sut.Column);
        // Current Lexer.cs will likely parse "\q" as "q". This test will fail.
        Assert.Equal(expectedValue, Encoding.UTF8.GetString(sut.Value));
    }

    [Fact]
    public void ReadString_UnterminatedEndingWithBackslash_ThrowsException()
    {
        /* Given */
        var source = "\"test\\\""; // Equivalent to "test\"
        var lexerInstance = Lexer.Create(source);
        Exception? actualException = null;

        /* When */
        try
        {
            lexerInstance.Advance();
        }
        catch (Exception ex)
        {
            actualException = ex;
        }

        /* Then */
        Assert.NotNull(actualException);
        Assert.IsAssignableFrom<Exception>(actualException);
        // Current Lexer.cs should throw this for an unterminated string.
        Assert.Contains("StringValue at 0 is not terminated.", actualException!.Message);
    }

    [Theory]
    [InlineData("\"\"\"  \\\"\"\"  \"\"\"", "  \"\"\"  ")] // Escaped triple quote results in literal triple quote
    [InlineData("\"\"\"\\\"\"\"\"\"\"", "\"\"\"")]      // Single escaped triple quote
    [InlineData("\"\"\"a\\\"\"\"b\"\"\"", "a\"\"\"b")]  // Escaped triple quote in content
    public void ReadBlockString_WithEscapedTripleQuotes(string inputString, string expectedValue)
    {
        /* Given */
        var source = inputString;
        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.BlockStringValue, sut.Kind);
        // The current Lexer.cs has flawed logic for escaped triple quotes.
        // It's expected to fail until Lexer.cs is fixed.
        // Spec says \""" becomes """.
        Assert.Equal(expectedValue, Encoding.UTF8.GetString(sut.Value));
    }

    [Theory]
    // Leading/trailing blank lines
    [InlineData("\"\"\"\n\n  Hello\n  World\n\n\"\"\"", "Hello\nWorld")]
    // Common indentation removal
    [InlineData("\"\"\"\n    Hello\n      World\n\"\"\"", "Hello\n  World")]
    // Varied indentation (common indent is 2, based on "  Line2")
    [InlineData("\"\"\"\n    Line1\n  Line2\n    Line3\n\"\"\"", "  Line1\nLine2\n  Line3")]
    // No common indentation (content starts at effective column 1)
    [InlineData("\"\"\"Hello\n  World\"\"\"", "Hello\n  World")] // No leading newline
    [InlineData("\"\"\"\nHello\n  World\n\"\"\"", "Hello\n  World")]   // With leading newline
    // Lines with only spaces/tabs (should be trimmed from ends, preserved if internal and non-empty after dedent)
    [InlineData("\"\"\"\n  \n  Hello\n    \n  World\n\"\"\"", "Hello\n  \nWorld")] // Corrected: internal non-empty line "  " is preserved
    // Uniform indentation with tabs (assuming tab contributes to common indentation)
    [InlineData("\"\"\"\n\tHello\n\t  World\n\"\"\"", "Hello\n  World")] // If tab is one unit of indent
    // Mixed spaces and tabs (behavior might be undefined or platform-dependent in some interpretations, spec implies equivalence)
    [InlineData("\"\"\"\n  \tHello\n  \t  World\n\"\"\"", "Hello\n  World")] // If "  \t" is common indent
    // String that is only whitespace common indentation
    [InlineData("\"\"\"\n  \n  \n\"\"\"", "")]
    // String with no content after initial quote
    [InlineData("\"\"\"\"\"\"", "")]
    // String with content on same line as opening quotes
    [InlineData("\"\"\"Hello\n  World\"\"\"", "Hello\n  World")]
    // String with content on same line and indentation
    [InlineData("\"\"\"  Hello\n    World\"\"\"", "Hello\n  World")]
    public void ReadBlockString_WithIndentationAndLineBreaks(string inputString, string expectedValue)
    {
        /* Given */
        var source = inputString;
        var sut = Lexer.Create(source);

        /* When */
        sut.Advance();

        /* Then */
        Assert.Equal(TokenKind.BlockStringValue, sut.Kind);
        // The current Lexer.cs does a raw slice and does not implement "Block String Value" processing.
        // These tests will fail until Lexer.cs is updated.
        Assert.Equal(expectedValue, Encoding.UTF8.GetString(sut.Value));
    }

    [Theory]
    [InlineData("\"\"\"test", 0)]
    [InlineData("\"\"\"test\"\"", 0)]
    [InlineData("\"\"\"test\\\"\"\"\"", 0)] // ends with an escaped triple quote, but no final closing triple quote
    public void ReadBlockString_Unterminated_ThrowsException(string inputString, int expectedStartOffset)
    {
        /* Given */
        var source = inputString;
        var lexerInstance = Lexer.Create(source);
        Exception? actualException = null;

        /* When */
        try
        {
            lexerInstance.Advance();
        }
        catch (Exception ex)
        {
            actualException = ex;
        }

        /* Then */
        Assert.NotNull(actualException);
        Assert.IsAssignableFrom<Exception>(actualException);
        Assert.Contains($"BlockString at {expectedStartOffset} is not terminated.", actualException!.Message);
    }
}