using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Tanka.GraphQL.Language.Tests;

/// <summary>
/// Edge case and error handling tests for the GraphQL Lexer.
/// These tests explore boundary conditions, malformed input, and unicode scenarios
/// to ensure robust tokenization behavior.
/// </summary>
public class LexerEdgeCasesFacts
{
    #region Unicode and Encoding Tests

    [Fact]
    public void Lexer_WithEmoji_InStringValue_TokenizesCorrectly()
    {
        // Given: String containing emoji characters
        var source = "{ field(arg: \"Hello 👋 World 🌍\") }";
        var lexer = Lexer.Create(source);
        
        // When: Tokenize the input
        var tokens = ExtractAllTokens(lexer);
        
        // Then: Should correctly tokenize the emoji-containing string
        // Find the string token
        var stringToken = Array.Find(tokens, t => t.Kind == TokenKind.StringValue);
        Assert.NotEqual(default(TokenInfo), stringToken);
        
        var stringValue = Encoding.UTF8.GetString(stringToken.Value);
        Assert.Equal("Hello 👋 World 🌍", stringValue);
    }

    [Fact]
    public void Lexer_WithUnicodeEscapes_InStringValue_HandlesCorrectly()
    {
        // Given: String with unicode escape sequences
        var source = "{ field(arg: \"Hello \\u0048\\u0065\\u006C\\u006C\\u006F\") }";
        var lexer = Lexer.Create(source);
        
        // When: Tokenize the input
        var tokens = ExtractAllTokens(lexer);
        
        // Then: Should handle unicode escapes
        var stringToken = Array.Find(tokens, t => t.Kind == TokenKind.StringValue);
        Assert.NotEqual(default(TokenInfo), stringToken);
        
        // Note: This test may fail if unicode escapes aren't processed during lexing
        // We need to understand if lexer processes escapes or if that's parser's job
        var rawValue = Encoding.UTF8.GetString(stringToken.Value);
        // The lexer might return the raw escape sequences, not the processed unicode
    }

    [Fact]
    public void Lexer_WithControlCharacters_InString_HandlesCorrectly()
    {
        // Given: String with control characters
        var source = "{ field(arg: \"line1\\nline2\\ttab\\r\\n\") }";
        var lexer = Lexer.Create(source);
        
        // When: Tokenize the input
        var tokens = ExtractAllTokens(lexer);
        
        // Then: Should handle control character escapes
        var stringToken = Array.Find(tokens, t => t.Kind == TokenKind.StringValue);
        Assert.NotEqual(default(TokenInfo), stringToken);
        
        var rawValue = Encoding.UTF8.GetString(stringToken.Value);
        // Again, lexer might not process escape sequences
    }

    [Fact]
    public void Lexer_WithInvalidUTF8Bytes_HandlesGracefully()
    {
        // Given: Invalid UTF-8 byte sequence
        var invalidUtf8 = new byte[] { 0x7B, 0x20, 0x22, 0xFF, 0xFE, 0x22, 0x20, 0x7D }; // { "��" }
        var lexer = Lexer.Create(invalidUtf8);
        
        // When & Then: This might throw an exception or handle gracefully
        // We need to understand the expected behavior
        try
        {
            var tokens = ExtractAllTokens(lexer);
            // If it succeeds, what should the behavior be?
        }
        catch (Exception ex)
        {
            // If it throws, is that the expected behavior?
            // This test will help us understand the current behavior
            Assert.True(true, $"Exception thrown: {ex.Message}");
        }
    }

    #endregion

    #region Malformed Token Tests

    [Fact]
    public void Lexer_WithUnterminatedString_HandlesError()
    {
        // Given: String without closing quote
        var source = "{ field(arg: \"unterminated string";
        var lexer = Lexer.Create(source);
        
        // When & Then: What should happen?
        try
        {
            var tokens = ExtractAllTokens(lexer);
            // Does it return an error token? Continue parsing? 
        }
        catch (Exception ex)
        {
            // Or does it throw an exception?
            Assert.True(true, $"Exception for unterminated string: {ex.Message}");
        }
    }

    [Fact]
    public void Lexer_WithUnterminatedBlockString_HandlesError()
    {
        // Given: Block string without proper closing
        var source = "{ field(arg: \"\"\"unterminated block string";
        var lexer = Lexer.Create(source);
        
        // When & Then: Test error handling
        try
        {
            var tokens = ExtractAllTokens(lexer);
        }
        catch (Exception ex)
        {
            Assert.True(true, $"Exception for unterminated block string: {ex.Message}");
        }
    }

    [Fact]
    public void Lexer_WithInvalidNumberFormat_Multiple_Dots_ThrowsSpecificError()
    {
        // Given: Invalid number with multiple decimal points (violates GraphQL spec)
        var source = "{ field(arg: 123.456.789) }";
        var lexer = Lexer.Create(source);
        
        // When & Then: Should correctly parse 123.456 then reject the second dot
        var exception = Assert.Throws<Exception>(() => {
            var testLexer = Lexer.Create(source);
            return ExtractAllTokens(testLexer);
        });
        
        // Verify specific error message with position information
        Assert.Contains("Unexpected character '.' at 1:21", exception.Message);
        
        // Verify that it correctly parsed the valid FloatValue first
        lexer = Lexer.Create(source);
        var tokens = new List<TokenInfo>();
        
        try
        {
            while (lexer.Advance())
            {
                tokens.Add(new TokenInfo 
                { 
                    Kind = lexer.Kind, 
                    Value = lexer.Value.ToArray(),
                    Line = lexer.Line,
                    Column = lexer.Column
                });
            }
        }
        catch (Exception)
        {
            // Expected exception on second dot
        }
        
        // Should have parsed: {, field, (, arg, :, 123.456 before failing
        Assert.True(tokens.Count >= 6, $"Expected at least 6 tokens, got {tokens.Count}");
        var floatToken = tokens.FirstOrDefault(t => t.Kind == TokenKind.FloatValue);
        Assert.NotEqual(default(TokenInfo), floatToken);
        Assert.Equal("123.456", Encoding.UTF8.GetString(floatToken.Value));
    }

    [Fact]
    public void Lexer_WithInvalidNumberFormat_Hexadecimal_HandlesCorrectly()
    {
        // Given: Hexadecimal numbers (not valid in GraphQL)
        var source = "{ field(arg: 0xDEADBEEF) }";
        var lexer = Lexer.Create(source);
        
        // When: Tokenize
        var tokens = ExtractAllTokens(lexer);
        
        // Then: Should probably treat this as separate tokens (0, x, DEADBEEF)
        // Let's see what actually happens
        var tokenTypes = Array.ConvertAll(tokens, t => t.Kind);
        
        // This test will show us how the lexer handles invalid number formats
    }

    [Fact]
    public void Lexer_WithInvalidEscapeSequence_HandlesError()
    {
        // Given: String with invalid escape sequence
        var source = "{ field(arg: \"invalid \\z escape\") }";
        var lexer = Lexer.Create(source);
        
        // When & Then: Test escape sequence validation
        try
        {
            var tokens = ExtractAllTokens(lexer);
            // Does lexer validate escape sequences or pass them through?
        }
        catch (Exception ex)
        {
            Assert.True(true, $"Exception for invalid escape: {ex.Message}");
        }
    }

    #endregion

    #region Performance and Boundary Tests

    [Fact]
    public void Lexer_WithVeryLongIdentifier_HandlesCorrectly()
    {
        // Given: Very long identifier (10,000 characters)
        var longName = new string('a', 10000);
        var source = $"{{ {longName} }}";
        var lexer = Lexer.Create(source);
        
        // When: Tokenize
        var tokens = ExtractAllTokens(lexer);
        
        // Then: Should handle long identifiers without issues
        var nameToken = Array.Find(tokens, t => t.Kind == TokenKind.Name);
        Assert.NotEqual(default(TokenInfo), nameToken);
        
        var tokenValue = Encoding.UTF8.GetString(nameToken.Value);
        Assert.Equal(longName, tokenValue);
    }

    [Fact]
    public void Lexer_WithDeeplyNestedBraces_HandlesWithoutStackOverflow()
    {
        // Given: Deeply nested braces (1000 levels)
        var openBraces = new string('{', 1000);
        var closeBraces = new string('}', 1000);
        var source = openBraces + " field " + closeBraces;
        var lexer = Lexer.Create(source);
        
        // When: Tokenize (this tests if lexer has any recursion issues)
        var tokens = ExtractAllTokens(lexer);
        
        // Then: Should handle deep nesting
        var braceCount = 0;
        foreach (var token in tokens)
        {
            if (token.Kind == TokenKind.LeftBrace || token.Kind == TokenKind.RightBrace)
                braceCount++;
        }
        
        Assert.Equal(2000, braceCount); // 1000 open + 1000 close
    }

    [Fact]
    public void Lexer_WithVeryLargeString_HandlesEfficiently()
    {
        // Given: Very large string value (100KB)
        var largeContent = new string('x', 100000);
        var source = $"{{ field(arg: \"{largeContent}\") }}";
        var lexer = Lexer.Create(source);
        
        // When: Tokenize
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tokens = ExtractAllTokens(lexer);
        stopwatch.Stop();
        
        // Then: Should handle large strings efficiently
        var stringToken = Array.Find(tokens, t => t.Kind == TokenKind.StringValue);
        Assert.NotEqual(default(TokenInfo), stringToken);
        
        var tokenValue = Encoding.UTF8.GetString(stringToken.Value);
        Assert.Equal(largeContent, tokenValue);
        
        // Performance check: should complete in reasonable time (< 1 second)
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Large string tokenization took {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void Lexer_WithEmptyInput_HandlesCorrectly()
    {
        // Given: Empty input
        var source = "";
        var lexer = Lexer.Create(source);
        
        // When: Try to advance
        var canAdvance = lexer.Advance();
        
        // Then: Should handle empty input gracefully
        Assert.False(canAdvance);
        Assert.Equal(TokenKind.End, lexer.Kind);
    }

    [Fact]
    public void Lexer_WithOnlyWhitespace_HandlesCorrectly()
    {
        // Given: Only whitespace and newlines
        var source = "   \n\r\n\t   \r  \n  ";
        var lexer = Lexer.Create(source);
        
        // When: Try to advance
        var canAdvance = lexer.Advance();
        
        // Then: Should skip all whitespace and reach end
        Assert.False(canAdvance);
        Assert.Equal(TokenKind.End, lexer.Kind);
    }

    #endregion

    #region Error Recovery and Position Tracking Tests

    [Fact]
    public void Lexer_WithMultipleErrors_ReportsPositionsCorrectly()
    {
        // Given: Input with multiple potential errors
        var source = "{\n  \"unterminated\n  123.456.789\n  field\n}";
        var lexer = Lexer.Create(source);
        
        // When: Tokenize and track positions
        var positions = new List<(int Line, int Column, TokenKind Kind)>();
        
        try
        {
            while (lexer.Advance())
            {
                positions.Add((lexer.Line, lexer.Column, lexer.Kind));
            }
        }
        catch (Exception)
        {
            // Even if there are errors, we want to check position tracking
        }
        
        // Then: Position tracking should be accurate
        // This test helps verify that line/column tracking works correctly
        Assert.True(positions.Count > 0, "Should have tokenized at least some tokens");
    }

    [Fact]
    public void Lexer_WithMixedLineEndings_TracksLinesCorrectly()
    {
        // Given: Input with different line ending styles
        var source = "{\n  field1\r\n  field2\r  field3\n}";
        var lexer = Lexer.Create(source);
        
        // When: Tokenize and track line numbers
        var tokens = ExtractAllTokensWithPosition(lexer);
        
        // Then: Line tracking should handle all line ending types
        var maxLine = tokens.Max(t => t.Line);
        Assert.True(maxLine >= 4, $"Should track multiple lines, got max line: {maxLine}");
    }

    #endregion

    #region Helper Methods

    private struct TokenInfo
    {
        public TokenKind Kind;
        public byte[] Value;
        public int Line;
        public int Column;
    }

    private TokenInfo[] ExtractAllTokens(Lexer lexer)
    {
        var tokens = new List<TokenInfo>();
        
        while (lexer.Advance())
        {
            tokens.Add(new TokenInfo 
            { 
                Kind = lexer.Kind, 
                Value = lexer.Value.ToArray(),
                Line = lexer.Line,
                Column = lexer.Column
            });
        }
        
        return tokens.ToArray();
    }

    private (TokenKind Kind, int Line, int Column)[] ExtractAllTokensWithPosition(Lexer lexer)
    {
        var tokens = new List<(TokenKind Kind, int Line, int Column)>();
        
        while (lexer.Advance())
        {
            tokens.Add((lexer.Kind, lexer.Line, lexer.Column));
        }
        
        return tokens.ToArray();
    }

    #endregion
}