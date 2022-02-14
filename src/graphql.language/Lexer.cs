using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Tanka.GraphQL.Language.Internal;

namespace Tanka.GraphQL.Language;

public ref struct Lexer
{
    private SpanReader _reader;

    private int _currentLineStart;

    public TokenKind Kind { get; private set; }

    public int Line { get; private set; }

    public int Column => Start - _currentLineStart + 1;

    public int Start { get; set; }

    public int Position => _reader.Position;

    public ReadOnlySpan<byte> Value { get; private set; }

    public bool IsExponential;

    public Lexer(in ReadOnlySpan<byte> span)
    {
        _reader = new SpanReader(span);
        _currentLineStart = 0;
        Kind = TokenKind.Start;
        Value = ReadOnlySpan<byte>.Empty;
        Line = 1;
        Start = 0;
        IsExponential = false;
    }

    public static Lexer Create(in ReadOnlySpan<byte> span)
    {
        return new Lexer(span);
    }

    public static Lexer Create(in string data)
    {
        return Create(Encoding.UTF8.GetBytes(data));
    }

    public bool Advance()
    {
        if (_reader.Position == -1)
            ReadBom();

        IgnoreWhitespace();

        if (_reader.TryPeek(out var code))
        {
#if GQL_COMMENTS
                if (code == Constants.Hash)
                {
                    ReadComment();
                    return true;
                }
#endif

            if (code == Constants.Quote)
            {
                ReadStringValue();
                return true;
            }

            if (Constants.IsPunctuator(code))
            {
                ReadPunctuator();
                return true;
            }

            if (_reader.IsNext(Constants.Spread.Span))
            {
                ReadSpreadPunctuator();
                return true;
            }

            if (Constants.IsNameStart(code))
            {
                ReadName();
                return true;
            }

            if (Constants.IsNumberStart(code))
            {
                ReadNumber();
                return true;
            }

            throw new Exception("Syntax error");
        }

        Start = Position;
        Kind = TokenKind.End;
        return false;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadStringValue()
    {
        Start = Position + 1;
        var valueStart = Start + 1;
        Kind = TokenKind.StringValue;

        // block string
        if (_reader.TrySkipNext(Constants.BlockString.Span))
        {
            valueStart += 2;
            while (!_reader.IsNext(Constants.BlockString.Span))
            {
                if (!_reader.Advance())
                    throw new Exception($"BlockString at {Start} is not terminated.");

                if (_reader.Span[_reader.Position] == Constants.Backslash)
                    // skip escaped block string quote
                    if (_reader.IsNext(Constants.BlockString.Span))
                        _reader.Advance(3);
            }

            // skip the end..
            var end = _reader.Position + 1;
            _reader.Advance(3);
            Kind = TokenKind.BlockStringValue;
            Value = _reader.Span.Slice(valueStart, end - valueStart);
            return;
        }

        // normal string
        // skip first quote
        if (!_reader.Advance())
            throw new Exception($"StringValue at {Start} is not terminated.");

        while (_reader.TryRead(out var code))
        {
            if (code != Constants.Quote)
                continue;

            // check escape
            if (Position > 0 && _reader.Span[Position - 1] == Constants.Backslash) continue;

            Value = _reader.Span.Slice(valueStart, Position - valueStart);
            return;
        }

        throw new Exception($"StringValue at {Start} is not terminated.");
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadNumber()
    {
        Kind = TokenKind.IntValue;
        Start = Position + 1;
        IsExponential = false;
        var isFloat = false;

        // starts with minus
        if (_reader.TryPeek(out var code))
            if (code == Constants.Minus)
                _reader.Advance();

        // starts with zero cannot follow with zero
        if (_reader.TryPeek(out code))
            if (code == '0')
            {
                _reader.Advance();

                if (_reader.TryPeek(out code))
                    if (code == '0')
                        throw new Exception(
                            $"Invalid number value starting at {Start}. " +
                            "Number starting with zero cannot be followed by zero.");
            }

        _reader.TryReadWhileAny(
            out var integerPart,
            Constants.IsDigit);

        if (_reader.TryPeek(out code))
        {
            if (code == Constants.e || code == Constants.E)
            {
                _reader.Advance();
                if (_reader.TryPeek(out var nextCode))
                    if (Constants.IsDigit[nextCode])
                        IsExponential = true;
            }

            if (code == Constants.Dot)
            {
                _reader.Advance();
                if (_reader.TryPeek(out var nextCode))
                    if (Constants.IsDigit[nextCode])
                        isFloat = true;
            }
        }

        if (IsExponential || isFloat)
        {
            Kind = TokenKind.FloatValue;

            _reader.TryReadWhileAny(
                out var fractionPart,
                Constants.IsDigit);

            if (_reader.TryPeek(out code))
                if (code == Constants.e || code == Constants.E)
                {
                    _reader.Advance();
                    if (_reader.TryPeek(out var nextCode))
                        if (Constants.IsDigit[nextCode])
                        {
                            IsExponential = true;
                            _reader.TryReadWhileAny(
                                out _,
                                Constants.IsDigit);
                        }
                }
        }

        Value = _reader.Span.Slice(Start, Position - Start + 1);
        IsExponential = true;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadSpreadPunctuator()
    {
        _reader.Advance();
        Kind = TokenKind.Spread;
        Start = _reader.Position;
        _reader.Advance(2);
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadName()
    {
        Kind = TokenKind.Name;
        Start = _reader.Position + 1;
        _reader.TryReadWhileAny(
            out var data,
            Constants.IsLetterOrDigitOrUnderscore);

        Value = data;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadPunctuator()
    {
        if (_reader.TryRead(out var code))
        {
            Start = Position;
            Kind = Constants.GetTokenKind(code);
        }
    }


    /// <summary>
    ///     comment
    /// </summary>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Conditional("GQL_COMMENTS")]
    private void ReadComment()
    {
#if GQL_COMMENTS
            Kind = TokenKind.Comment;

            // skip #
            _reader.TryRead(out _);
            Start = Position;

            // there might be space after the hash or not
            IgnoreWhitespace();

            // read until \r or \n or end
            _reader.TryReadWhileNotAny(
                out var data,
                Constants.IsReturnOrNewLine);

            Value = data;
#endif
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IgnoreWhitespace()
    {
        while (_reader.TryPeek(out var code))
            switch (code)
            {
#if !GQL_COMMENTS
                case Constants.Hash:
                    _reader.TryReadWhileNotAny(out _, Constants.IsReturnOrNewLine);
                    break;
#endif
                case Constants.NewLine:
                    _reader.Advance();
                    StartNewLine();
                    break;
                case Constants.Return:
                    _reader.Advance();
                    break;
                case Constants.Tab:
                case Constants.Space:
                case Constants.Comma:
                    _reader.Advance();
                    break;
                default:
                    return;
            }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StartNewLine()
    {
        Line++;
        _currentLineStart = _reader.Position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadBom()
    {
        // skip bom
        _reader.TrySkipNext(Constants.Bom.Span);
    }
}