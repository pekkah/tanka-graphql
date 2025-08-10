using System;

namespace Tanka.GraphQL.Language;

/// <summary>
/// Exception thrown when parsing GraphQL documents fails
/// </summary>
public class ParseException : Exception
{
    public ParseException(string message) : base(message)
    {
    }

    public ParseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ParseException(string message, int line, int column) : base(message)
    {
        Line = line;
        Column = column;
    }

    /// <summary>
    /// Line number where the parsing error occurred
    /// </summary>
    public int? Line { get; }

    /// <summary>
    /// Column number where the parsing error occurred
    /// </summary>
    public int? Column { get; }
}