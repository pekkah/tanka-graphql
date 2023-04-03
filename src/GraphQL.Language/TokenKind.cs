namespace Tanka.GraphQL.Language;

public enum TokenKind
{
    Start,
    End,
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
    RightBrace,
    Spread,
    Name,
    IntValue,
    FloatValue,
    StringValue,
    BlockStringValue,
#if GQL_COMMENTS
        Comment
#endif
}