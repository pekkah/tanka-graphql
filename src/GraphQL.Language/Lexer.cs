using System;
using System.Collections.Generic; // Added for List<byte>
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

    public Lexer(ReadOnlySpan<byte> span)
    {
        _reader = new SpanReader(span);
        _currentLineStart = 0;
        Kind = TokenKind.Start;
        Value = ReadOnlySpan<byte>.Empty;
        Line = 1;
        Start = 0;
        IsExponential = false;
    }

    public static Lexer Create(ReadOnlySpan<byte> span)
    {
        return new Lexer(span);
    }

    public static Lexer Create(string data)
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

            /*if (_reader.IsNext(Constants.EscapedQuote))
            {
                ReadStringValue();
                return true;
            }*/

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
        if (_reader.TrySkipNext(Constants.BlockString.Span)) // Consumes opening """
        {
            this.Kind = TokenKind.BlockStringValue;
            // this.Start is already set to the beginning of the opening """ by the caller or earlier logic

            var rawBuilder = new List<byte>();
            // var rawBuilder = new List<byte>(); // Removed duplicate declaration
            while (true)
            {
                if (_reader.IsNext(Constants.BlockString.Span))
                {
                    _reader.Advance(3); // Consume closing """
                    break; 
                }

                if (!_reader.TryPeek(out byte currentChar))
                {
                    throw new Exception($"BlockString at {this.Start} is not terminated.");
                }

                if (currentChar == Constants.Backslash)
                {
                    bool isEscapedTripleQuote = false;
                    // currentChar is at _reader.Span[_reader.Position + 1]
                    // We need to check 3 characters after currentChar for '"""'
                    // These are at indices:
                    // _reader.Position + 2 (for first quote)
                    // _reader.Position + 3 (for second quote)
                    // _reader.Position + 4 (for third quote)
                    // So, we need to ensure _reader.Position + 4 is a valid index.
                    // Valid index means < _reader.Span.Length. So, _reader.Position + 4 < _reader.Span.Length
                    // Or, _reader.Span.Length >= _reader.Position + 5
                    if ((_reader.Position + 1 + 3) < _reader.Span.Length) // Check if 3 chars exist after currentChar
                    {
                        if (_reader.Span[_reader.Position + 2] == (byte)'"' &&
                            _reader.Span[_reader.Position + 3] == (byte)'"' &&
                            _reader.Span[_reader.Position + 4] == (byte)'"')
                        {
                            isEscapedTripleQuote = true;
                        }
                    }

                    if (isEscapedTripleQuote)
                    {
                        _reader.Advance(); // Consume the backslash (currentChar)
                        _reader.Advance(); // Consume the first quote after backslash
                        _reader.Advance(); // Consume the second quote
                        _reader.Advance(); // Consume the third quote
                        rawBuilder.Add((byte)'"');
                        rawBuilder.Add((byte)'"');
                        rawBuilder.Add((byte)'"');
                    }
                    else
                    {
                        _reader.Advance(); // Consume the backslash (currentChar)
                        rawBuilder.Add(currentChar); // Add the backslash literally
                    }
                }
                else // Not a backslash
                {
                    _reader.Advance(); // Consume currentChar
                    rawBuilder.Add(currentChar);
                }
            }
            this.Value = ProcessBlockString(new ReadOnlySpan<byte>(rawBuilder.ToArray()));
            return;
        }

        // normal string
        // skip first quote
        if (!_reader.Advance()) // Consumes the opening quote. _reader.Position is now at the index of the opening quote.
            throw new Exception($"StringValue at {this.Start} is not terminated."); // Should only happen for empty source after quote

        var valueBuilder = new List<byte>();
        while (true)
        {
            if (!_reader.TryRead(out byte code)) // Read next char, advances _reader.Position
            {
                // End of input before closing quote
                throw new Exception($"StringValue at {this.Start} is not terminated.");
            }

            if (code == Constants.Quote)
            {
                // End of string
                this.Value = new ReadOnlySpan<byte>(valueBuilder.ToArray());
                return;
            }

            if (code == Constants.Backslash)
            {
                if (!_reader.TryRead(out byte escapedChar)) // Read char after backslash
                {
                    // String ends with a raw backslash e.g. "test\"
                    throw new Exception($"StringValue at {this.Start} is not terminated.");
                }

                switch (escapedChar)
                {
                    case (byte)'"': valueBuilder.Add((byte)'"'); break;
                    case (byte)'\\': valueBuilder.Add((byte)'\\'); break;
                    case (byte)'/': valueBuilder.Add((byte)'/'); break;
                    case (byte)'b': valueBuilder.Add((byte)'\b'); break;
                    case (byte)'f': valueBuilder.Add((byte)'\f'); break;
                    case (byte)'n': valueBuilder.Add((byte)'\n'); break;
                    case (byte)'r': valueBuilder.Add((byte)'\r'); break;
                    case (byte)'t': valueBuilder.Add((byte)'\t'); break;
                    case (byte)'u':
                        int h1, h2, h3, h4;
                        if (!_reader.TryRead(out byte c1) || !TryParseHexChar(c1, out h1) ||
                            !_reader.TryRead(out byte c2) || !TryParseHexChar(c2, out h2) ||
                            !_reader.TryRead(out byte c3) || !TryParseHexChar(c3, out h3) ||
                            !_reader.TryRead(out byte c4) || !TryParseHexChar(c4, out h4))
                        {
                            throw new Exception($"Invalid Unicode escape sequence in string starting at {this.Start}.");
                        }
                        var unicodeValue = (h1 << 12) | (h2 << 8) | (h3 << 4) | h4;
                        var chars = char.ConvertFromUtf32(unicodeValue);
                        valueBuilder.AddRange(Encoding.UTF8.GetBytes(chars));
                        break;
                    default:
                        // Non-standard escape: add backslash and the character itself
                        valueBuilder.Add(Constants.Backslash);
                        valueBuilder.Add(escapedChar);
                        break;
                }
            }
            else // Not a backslash, not a quote
            {
                valueBuilder.Add(code);
            }
        }
    }

    private static bool TryParseHexChar(byte c, out int value)
    {
        if (c >= '0' && c <= '9') { value = c - '0'; return true; }
        if (c >= 'a' && c <= 'f') { value = c - 'a' + 10; return true; }
        if (c >= 'A' && c <= 'F') { value = c - 'A' + 10; return true; }
        value = 0;
        return false;
    }

    private static ReadOnlySpan<byte> ProcessBlockString(ReadOnlySpan<byte> rawBytes)
    {
        if (rawBytes.IsEmpty)
        {
            return ReadOnlySpan<byte>.Empty;
        }

        string rawString = Encoding.UTF8.GetString(rawBytes);
        string normalizedString = rawString.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = new List<string>(normalizedString.Split('\n'));

        int? commonIndent = null;
        List<string> linesForProcessing = new List<string>();

        // Determine lines relevant for common indentation calculation and initial processing
        bool firstLineIgnoredForIndentation = false;
        if (lines.Count > 0 && IsLineBlank(lines[0]))
        {
            firstLineIgnoredForIndentation = true;
            // The first line, if blank, is still part of content lines for now,
            // but its indent doesn't define commonIndent.
        }

        for(int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            linesForProcessing.Add(line); // Keep the line for now

            if (i == 0 && firstLineIgnoredForIndentation) continue; // Skip first blank line for indent calc
            if (IsLineBlank(line)) continue; // Skip other blank lines for indent calc

            int indent = GetLeadingWhitespaceCount(line);
            if (commonIndent == null || indent < commonIndent.Value)
            {
                commonIndent = indent;
            }
        }
        
        commonIndent ??= 0; // If no non-blank lines to set commonIndent, it's 0.
        if (lines.Count == 1 && !firstLineIgnoredForIndentation && IsLineBlank(lines[0])) commonIndent = 0; // if only one line and it is blank (e.g. """  """)
        
        // If there's only one line of content after initial blank line trimming, its own leading whitespace should not be removed by "common" logic.
        // linesForProcessing contains lines from first non-blank to last non-blank (inclusive original list)
        // The actual content lines to be joined are firstContentLineIndex to lastContentLineIndex from resultLines (after dedent)
        // This is tricky. Let's adjust commonIndent if the number of lines that *will form the final block* is 1.
        // The number of lines in linesForProcessing that are not themselves blank lines is a better proxy.
        int actualContentLineCount = 0;
        foreach(string line in linesForProcessing) { if (!IsLineBlank(line)) actualContentLineCount++; }
        if (actualContentLineCount <= 1) commonIndent = 0;


        var resultLines = new List<string>();
        for (int i = 0; i < linesForProcessing.Count; i++)
        {
            string line = linesForProcessing[i];
            if (commonIndent.Value > 0)
            {
                int actualLeadingWhitespace = GetLeadingWhitespaceCount(line);
                int numToRemove = Math.Min(actualLeadingWhitespace, commonIndent.Value);
                resultLines.Add(line.Substring(numToRemove));
            }
            else
            {
                resultLines.Add(line);
            }
        }

        // Trim leading blank lines from result
        int firstContentLineIndex = -1;
        for (int i = 0; i < resultLines.Count; i++)
        {
            if (!IsLineBlank(resultLines[i]))
            {
                firstContentLineIndex = i;
                break;
            }
        }

        if (firstContentLineIndex == -1) // All lines were blank
        {
            return ReadOnlySpan<byte>.Empty;
        }

        // Trim trailing blank lines from result
        int lastContentLineIndex = -1;
        for (int i = resultLines.Count - 1; i >= firstContentLineIndex; i--)
        {
            if (!IsLineBlank(resultLines[i]))
            {
                lastContentLineIndex = i;
                break;
            }
        }
        
        var finalLines = new List<string>();
        if (firstContentLineIndex <= lastContentLineIndex) // Ensure there's content
        {
           for(int i = firstContentLineIndex; i <= lastContentLineIndex; i++)
           {
               finalLines.Add(resultLines[i]);
           }
        }
        
        return Encoding.UTF8.GetBytes(string.Join("\n", finalLines));
    }

    private static bool IsLineBlank(string line)
    {
        foreach (char c in line)
        {
            if (c != ' ' && c != '\t') return false;
        }
        return true;
    }

    private static int GetLeadingWhitespaceCount(string line)
    {
        int count = 0;
        foreach (char c in line)
        {
            if (c == ' ' || c == '\t') count++;
            else break;
        }
        return count;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadNumber()
    {
        this.Kind = TokenKind.IntValue;
        this.Start = _reader.Position + 1; // _reader.Position is before the first char of the number
        this.IsExponential = false; // Lexer's public field
        bool isFloat = false;       // Local flag for decimal point

        // Optional Minus
        if (_reader.TryPeek(out var code) && code == Constants.Minus)
        {
            _reader.Advance(); // Consume '-'
        }

        // Integer Part & Leading Zero Check
        if (!_reader.TryPeek(out var firstCharOrDigit) || !Constants.IsDigit[firstCharOrDigit])
        {
            // Must have at least one digit after an optional sign.
            throw new Exception($"Invalid number: missing digits, starting at {this.Start}.");
        }

        if (firstCharOrDigit == '0')
        {
            _reader.Advance(); // Consume '0'
            // Check for leading zero error: "0" followed by another digit is invalid (e.g., "01", "00").
            // Allowed: "0.1", "0e1", "0E1", or just "0" followed by non-digit/EOF.
            if (_reader.TryPeek(out var nextCharAfterZero) && Constants.IsDigit[nextCharAfterZero])
            {
                throw new Exception($"Invalid number: leading zero followed by other digits, starting at {this.Start}.");
            }
            // _reader.Position is now at '0'.
        }
        else // firstCharOrDigit is '1'-'9'
        {
            _reader.Advance(); // Consume the first digit ('1'-'9')
            // Consume remaining digits of the integer part
            while (_reader.TryPeek(out var nextDigit) && Constants.IsDigit[nextDigit])
            {
                _reader.Advance();
            }
            // _reader.Position is now at the last digit of the integer part.
        }

        // Fractional Part
        if (_reader.TryPeek(out var charAfterInt) && charAfterInt == Constants.Dot)
        {
            if (isFloat) // Should be unreachable if logic is correct, but as a safeguard.
            {
                throw new Exception($"Invalid number: multiple decimal points, starting at {this.Start}.");
            }
            isFloat = true;
            this.Kind = TokenKind.FloatValue;
            _reader.Advance(); // Consume '.'
            // _reader.Position is now at '.'.

            // After '.', there must be at least one digit.
            if (!_reader.TryPeek(out var charAfterDot) || !Constants.IsDigit[charAfterDot])
            {
                throw new Exception($"Invalid number: decimal part missing digits, starting at {this.Start}.");
            }

            // Consume digits of the fractional part
            while (_reader.TryPeek(out var nextFracDigit) && Constants.IsDigit[nextFracDigit])
            {
                _reader.Advance();
            }
            // _reader.Position is now at the last digit of the fractional part.
        }

        // After processing a potential fractional part, check if another dot follows.
        // This handles cases like "1.2.3". After "1.2" is parsed, if a "." is next, it's an error.
        if (isFloat && _reader.TryPeek(out var charAfterFracScan) && charAfterFracScan == Constants.Dot)
        {
            throw new Exception($"Invalid number: multiple decimal points, starting at {this.Start}.");
        }

        // Exponent Part
        if (_reader.TryPeek(out var charAfterFracOrInt) && (charAfterFracOrInt == Constants.e || charAfterFracOrInt == Constants.E))
        {
            if (this.IsExponential) // Check if 'e' or 'E' has already been processed.
            {
                throw new Exception($"Invalid number: multiple exponent parts, starting at {this.Start}.");
            }
            this.IsExponential = true; // Mark that an exponent part is being processed.
            this.Kind = TokenKind.FloatValue;
            _reader.Advance(); // Consume 'e' or 'E'
            // _reader.Position is now at 'e' or 'E'.

            // Optional sign for exponent
            if (_reader.TryPeek(out var potentialSign) && (potentialSign == Constants.Plus || potentialSign == Constants.Minus))
            {
                _reader.Advance(); // Consume sign
                // _reader.Position is now at the exponent's sign.
            }

            // After 'e'/'E' (and optional sign), there must be at least one digit.
            if (!_reader.TryPeek(out var charAfterExpSign) || !Constants.IsDigit[charAfterExpSign])
            {
                throw new Exception($"Invalid number: exponent part missing digits, starting at {this.Start}.");
            }

            // Consume digits of the exponent part
            while (_reader.TryPeek(out var nextExpDigit) && Constants.IsDigit[nextExpDigit])
            {
                _reader.Advance();
            }
            // _reader.Position is now at the last digit of the exponent part.
        }

        // After processing a potential exponent part, check if another 'e' or 'E' follows.
        // This handles cases like "1e2e3". After "1e2" is parsed, if an 'e' is next, it's an error.
        if (this.IsExponential && _reader.TryPeek(out var charAfterExpScan) && (charAfterExpScan == Constants.e || charAfterExpScan == Constants.E))
        {
            throw new Exception($"Invalid number: multiple exponent parts, starting at {this.Start}.");
        }

        // Set the value of the token.
        // _reader.Position is the index of the last char of the number.
        // this.Start is the index of the first char of the number.
        this.Value = _reader.Span.Slice(this.Start, _reader.Position - this.Start + 1);
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

            // When ReadComment is called:
            // - this.Start has already been set by Advance() to the position of the '#'.
            // - _reader.Position is currently *before* the '#'.
            
            this.Kind = TokenKind.Comment; // Ensure Kind is set for the comment token
            _reader.Advance(); // Consume the '#'. Now _reader.Position is at the '#'.
            
            int commentContentStartIndex = _reader.Position + 1; // Content starts after '#'
            int commentContentEndIndexHelper = _reader.Position; // Tracks end of content; initially at '#'

            while (_reader.TryPeek(out var currentChar))
            {
                if (currentChar == Constants.NewLine || currentChar == Constants.Return)
                {
                    break; // Stop *before* consuming the newline.
                }
                _reader.Advance(); // Consume the content character.
                commentContentEndIndexHelper = _reader.Position; // Mark the last consumed character's position.
            }

            if (commentContentStartIndex > commentContentEndIndexHelper + 1) 
            {
                this.Value = ReadOnlySpan<byte>.Empty;
            }
            else
            {
                this.Value = _reader.Span.Slice(commentContentStartIndex, (commentContentEndIndexHelper + 1) - commentContentStartIndex);
            }

            // Consume the line terminator and update line state.
            if (_reader.TryPeek(out var potentialNewLine))
            {
                if (potentialNewLine == Constants.Return) // \r
                {
                    _reader.Advance(); // Consume '\r'
                    if (_reader.TryPeek(out var nextChar) && nextChar == Constants.NewLine) // \n for \r\n
                    {
                        _reader.Advance(); // Consume '\n'
                    }
                    StartNewLine(); // Update line state
                }
                else if (potentialNewLine == Constants.NewLine) // \n
                {
                    _reader.Advance(); // Consume '\n'
                    StartNewLine(); // Update line state
                }
                // If not \r or \n (e.g. comment at EOF), do nothing with line state here.
            }
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
                case Constants.Backslash:

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