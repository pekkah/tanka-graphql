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
    [InlineData("\"test \\\"\"", "test \\\"")]
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
    [InlineData("\"\"\"test \\\"\"\"\"\"\"", "test \\\"\"\"")]
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
}