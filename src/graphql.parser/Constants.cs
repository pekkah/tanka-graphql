using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Tanka.GraphQL.Language
{
    public static class Constants
    {
        public const byte Hyphen = (byte) '-';
        public const byte Underscore = (byte) '_';
        public const byte Plus = (byte) '+';
        public const byte Minus = (byte) '-';
        public const byte Backslash = (byte) '\\';
        public const byte ForwardSlash = (byte) '/';
        public const byte Backspace = (byte) '\b';
        public const byte FormFeed = (byte) '\f';
        public const byte ExclamationMark = (byte) '!';
        public const byte Dollar = (byte) '$';
        public const byte Ampersand = (byte) '&';
        public const byte LeftParenthesis = (byte) '(';
        public const byte RightParenthesis = (byte) ')';
        public const byte Colon = (byte) ':';
        public const byte Equal = (byte) '=';
        public const byte At = (byte) '@';
        public const byte LeftBracket = (byte) '[';
        public const byte RightBracket = (byte) ']';
        public const byte LeftBrace = (byte) '{';
        public const byte RightBrace = (byte) '}';
        public const byte Pipe = (byte) '|';
        public const byte Dot = (byte) '.';
        public const byte Space = (byte) ' ';
        public const byte Hash = (byte) '#';
        public const byte Tab = (byte) '\t';
        public const byte NewLine = (byte) '\n';
        public const byte Return = (byte) '\r';
        public const byte Quote = (byte) '"';
        public const byte Comma = (byte) ',';

        // ReSharper disable once InconsistentNaming
        public const byte e = (byte) 'e';
        public const byte E = (byte) 'E';

        private static readonly bool[] _isPunctuator = new bool[256];
        
        public static readonly bool[] IsLetterOrUnderscore = new bool[256];
        public static readonly bool[] IsLetterOrDigitOrUnderscore = new bool[256];
        public static readonly  bool[] IsReturnOrNewLine = new bool[256];
        public static readonly  bool[] IsMinusOrNonZeroDigit = new bool[256];
        public static readonly  bool[] IsDigit = new bool[256];

        public static ReadOnlyMemory<byte> Bom = new ReadOnlyMemory<byte>(
            Encoding.UTF8.GetPreamble());

        public static ReadOnlyMemory<byte> Spread = new ReadOnlyMemory<byte>(new[]
        {
            Dot,
            Dot,
            Dot
        });

        public static ReadOnlyMemory<byte> BlockString = new ReadOnlyMemory<byte>(new[]
        {
            Quote,
            Quote,
            Quote
        });

        public static ReadOnlyMemory<byte> ReturnAndNewLine = new ReadOnlyMemory<byte>(new[]
        {
            Return,
            NewLine
        });

        public static ReadOnlyMemory<byte> NewLineMemory = new ReadOnlyMemory<byte>(new[]
        {
            NewLine
        });

        public static ReadOnlyMemory<byte> Punctuators = new ReadOnlyMemory<byte>(new[]
        {
            ExclamationMark,
            Dollar,
            Ampersand,
            LeftParenthesis,
            RightParenthesis,
            Colon,
            Equal,
            At,
            LeftBracket,
            RightBracket,
            LeftBrace,
            Pipe,
            RightBrace
        });


        static Constants()
        {
            foreach (var punctuator in Punctuators.Span)
                _isPunctuator[punctuator] = true;

            /* NameStart */
            for (var c = 'a'; c <= 'z'; c++) 
                IsLetterOrUnderscore[c] = true;

            for (var c = 'A'; c <= 'Z'; c++) 
                IsLetterOrUnderscore[c] = true;

            IsLetterOrUnderscore['_'] = true;

            /* NameContinue */
            for (var c = 'a'; c <= 'z'; c++) 
                IsLetterOrDigitOrUnderscore[c] = true;

            for (var c = 'A'; c <= 'Z'; c++) 
                IsLetterOrDigitOrUnderscore[c] = true;

            for(char d = '0'; d <= '9'; d++)
            {
                IsLetterOrDigitOrUnderscore[d] = true;
            }

            IsLetterOrDigitOrUnderscore['_'] = true;

            /* Return Or NewLine */
            IsReturnOrNewLine[Return] = true;
            IsReturnOrNewLine[NewLine] = true;

            /* Digit */
            for(char d = '0'; d <= '9'; d++)
            {
                IsDigit[d] = true;
            }

            /* Minus Or Digit */
            for(char d = '1'; d <= '9'; d++)
            {
                IsMinusOrNonZeroDigit[d] = true;
            }

            IsMinusOrNonZeroDigit['-'] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPunctuator(in byte code)
        {
            return _isPunctuator[code];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TokenKind GetTokenKind(in byte code)
        {
            return code switch
            {
                ExclamationMark => TokenKind.ExclamationMark,
                Dollar => TokenKind.Dollar,
                Ampersand => TokenKind.Ampersand,
                LeftParenthesis => TokenKind.LeftParenthesis,
                RightParenthesis => TokenKind.RightParenthesis,
                Colon => TokenKind.Colon,
                Equal => TokenKind.Equal,
                At => TokenKind.At,
                LeftBracket => TokenKind.LeftBracket,
                RightBracket => TokenKind.RightBracket,
                LeftBrace => TokenKind.LeftBrace,
                Pipe => TokenKind.Pipe,
                RightBrace => TokenKind.RightBrace,
                _ => throw new InvalidOperationException($"Code '{code}' is not any TokenKind")
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNameStart(in byte code)
        {
            return IsLetterOrUnderscore[code];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumberStart(in byte value)
        {
            return IsMinusOrNonZeroDigit[value];
        }
    }
}